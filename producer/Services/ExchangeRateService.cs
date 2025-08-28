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
        public async Task<ExchangePackage> DocumentPackage(ExchangePackageRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null || request?.currencies == null)
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");
            }

            var bag = new System.Collections.Concurrent.ConcurrentBag<ExchangeRate>();

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

            await packagesCollection.InsertOneAsync(package);
            return package;
        }
    }
}