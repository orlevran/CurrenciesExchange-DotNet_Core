using producer.Models.DTOs;
using System.Collections.Concurrent;

namespace producer.Services
{
    public interface IKafkaStreamManager
    {
        string Start(StartKafkaStreamRequest req, CancellationToken ct);
        bool Stop(string streamId);
        IReadOnlyCollection<string> List();
    }
    
    public sealed class KafkaStreamManager(
    ILogger<KafkaStreamManager> log,
    IKafkaProducerService producer,
    IExchangeRateService exchangeRateService) : IKafkaStreamManager
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _streams = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Start a new Kafka stream with given pairs and interval, returning its ID
        /// </summary>
        /// <param name="req"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string Start(StartKafkaStreamRequest req, CancellationToken ct)
        {
            // Enforce minimum interval of 60 seconds
            var interval = Math.Max(60, req.IntervalInSeconds);

            // Generate a unique ID for the stream
            var id = Guid.NewGuid().ToString("N");

            // Create a cancellation token source for this stream
            var cts = new CancellationTokenSource();
            if (!_streams.TryAdd(id, cts)) throw new InvalidOperationException("Could not create stream.");

            // Start the stream loop in a background task
            _ = Task.Run(() => RunLoop(id, req, interval, cts.Token), cts.Token);
            return id;
        }

        public bool Stop(string streamId)
        {
            if (_streams.TryRemove(streamId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                return true;
            }
            return false;
        }

        public IReadOnlyCollection<string> List() => _streams.Keys.ToArray();

        /// <summary>
        /// The main loop for a Kafka stream
        /// runs until cancelled
        /// </summary>
        /// <param name="id"></param>
        /// <param name="req"></param>
        /// <param name="intervalSeconds"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task RunLoop(string id, StartKafkaStreamRequest req, int intervalSeconds, CancellationToken ct)
        {
            // Create a periodic timer for the stream interval
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
            var consecutiveFailures = 0;

            try
            {
                try
                {
                    // Produce once immediately on start
                    await ProduceOnce(req.Pairs, ct);
                    consecutiveFailures = 0;
                }
                catch (Exception ex)
                {
                    /*
                        On failure, log and back off with exponential delay
                        up to a maximum of 60 seconds
                        This prevents tight failure loops
                        but allows recovery if the issue resolves
                        e.g. temporary network or API issues
                        The backoff is 2^n seconds, capped at 60s
                        where n is the number of consecutive failures
                        This gives 1,2,4,8,16,32,60,60,... seconds delays
                        after each failure:
                        1st: 2^1=2s
                        2nd: 2^2=4s
                        3rd: 2^3=8s
                        4th: 2^4=16s
                        5th: 2^5=32s
                        6th: 2^6=64s -> capped to 60s
                        7th+: 60s
                        This helps avoid overwhelming the external API or Kafka
                        while still attempting to recover periodically
                        The counter resets to 0 on a successful ProduceOnce call
                    */
                    consecutiveFailures++;
                    // Log the failure and backoff
                    var backoff = TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, Math.Min(6, consecutiveFailures))));
                    log.LogError(ex, "ProduceOnce failed (#{Count}). Backing off {Backoff}", consecutiveFailures, backoff);
                    try
                    {
                        await Task.Delay(backoff, ct);
                    }
                    // If the delay was canceled, we can exit the loop
                    catch (OperationCanceledException) { }
                }
                /*
                    Main loop, waiting for the next timer tick
                    On each tick, attempt to produce once
                    If it fails, log and back off as above
                    The loop exits when the timer is disposed or cancelled
                    ReSharper disable once MethodSupportsCancellation
                    ReSharper disable once AccessToDisposedClosure
                    ReSharper disable once AsyncMethodInSyncContext
                    various ReSharper warnings are disabled here
                    because this is a fire-and-forget background task
                */
                while (await timer.WaitForNextTickAsync(ct))
                {
                    try
                    {
                        await ProduceOnce(req.Pairs, ct);
                        consecutiveFailures = 0;
                    }
                    catch (Exception ex)
                    {
                        // On failure, log and back off with exponential delay as above
                        consecutiveFailures++;
                        var backoff = TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, Math.Min(6, consecutiveFailures))));
                        log.LogError(ex, "ProduceOnce failed (#{Count}). Backing off {Backoff}", consecutiveFailures, backoff);
                        try
                        {
                            await Task.Delay(backoff, ct);
                        }
                        catch (OperationCanceledException) { }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                log.LogInformation("Stream {Id} cancelled", id);
            }
        }

        /// <summary>
        /// Produce exchange rates for given pairs once
        /// </summary>
        /// <param name="pairs"></param>
        /// <param name="ct"></param>
        private async Task ProduceOnce(IEnumerable<PairDto> pairs, CancellationToken ct)
        {
            var package = await exchangeRateService.DocumentPackage(new ExchangePackageRequest
            {
                currencies = pairs.Select(p => new Tuple<string, string>(p.From, p.To)).ToList()
            }, ct);

            if (package?.rates is { Count: > 0 })
            {
                // Produce the message to Kafka
                // This is an async call that returns when the message is acknowledged
                await producer.ProduceMessageAsync(package, ct);
                log.LogInformation("Produced package with {count} rates at {Time}", package.rates.Count, package.time);
            }
            else
            {
                log.LogWarning("DocumentPackage returned no rates");
            }
        }
    }
}