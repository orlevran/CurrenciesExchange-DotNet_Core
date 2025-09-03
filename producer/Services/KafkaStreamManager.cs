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
        public string Start(StartKafkaStreamRequest req, CancellationToken ct)
        {
            var interval = Math.Max(60, req.IntervalInSeconds);
            var id = Guid.NewGuid().ToString("N");
            var cts = new CancellationTokenSource();
            if (!_streams.TryAdd(id, cts)) throw new InvalidOperationException("Could not create stream.");

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

        private async Task RunLoop(string id, StartKafkaStreamRequest req, int intervalSeconds, CancellationToken ct)
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
            var consecutiveFailures = 0;

            try
            {
                try
                {
                    await ProduceOnce(req.Pairs, ct);
                    consecutiveFailures = 0;
                }
                catch (Exception ex)
                {
                    consecutiveFailures++;
                    var backoff = TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, Math.Min(6, consecutiveFailures))));
                    log.LogError(ex, "ProduceOnce failed (#{Count}). Backing off {Backoff}", consecutiveFailures, backoff);
                    try
                    {
                        await Task.Delay(backoff, ct);
                    }
                    catch (OperationCanceledException) { }
                }

                while (await timer.WaitForNextTickAsync(ct))
                {
                    try
                    {
                        await ProduceOnce(req.Pairs, ct);
                        consecutiveFailures = 0;
                    }
                    catch (Exception ex)
                    {
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

        private async Task ProduceOnce(IEnumerable<PairDto> pairs, CancellationToken ct)
        {
            var package = await exchangeRateService.DocumentPackage(new ExchangePackageRequest
            {
                currencies = pairs.Select(p => new Tuple<string, string>(p.From, p.To)).ToList()
            }, ct);

            if (package?.rates is { Count: > 0 })
            {
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