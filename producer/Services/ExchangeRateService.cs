using producer.Models;
using producer.Models.DTOs;
using producer.Services.ExchangeProviders;
using producer.Configurations;
using MongoDB.Driver;

namespace producer.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IMongoCollection<ExchangePackage> packagesCollection;
        private readonly IExchangeClient exchangeClient;
        private readonly ActiveExchangeOptions active;

        public ExchangeRateService(IMongoDatabase database, IExchangeClient _exchangeClient, ActiveExchangeOptions _active)
        {
            packagesCollection = database.GetCollection<ExchangePackage>("ExchangePackages");
            exchangeClient = _exchangeClient;
            active = _active;
        }

        /// <summary>
        /// Fetch exchange rates for given currency pairs from external API and store in DB
        /// </summary>
        /// <param name="request"></param>
        public async Task<ExchangePackage> DocumentPackage(ExchangePackageRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || request?.currencies == null)
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");
            }

            // Use a thread-safe collection to store results from parallel tasks
            var bag = new System.Collections.Concurrent.ConcurrentBag<ExchangeRate>();

            // Fetch rates in parallel for each currency pair
            await Parallel.ForEachAsync(request.currencies, cancellationToken, async (tuple, ct) =>
            {
                var from = tuple.Item1;
                var to = tuple.Item2;

                try
                {
                    var rate = await exchangeClient.GetRatesAsync(active.Key, from, to, ct);
                    bag.Add(rate);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed {from}->{to}: {ex.Message}");
                }
            });

            var package = new ExchangePackage { rates = bag.ToList() };

            // Insert the package into MongoDB
            await packagesCollection.InsertOneAsync(package);
            return package;
        }
    }
}