using consumer.Models;
using consumer.Models.DTOs;

using MongoDB.Driver;

namespace consumer.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly IMongoCollection<ExchangePackage> packagesCollection;
        public ConsumerService(IMongoDatabase database)
        {
            packagesCollection = database.GetCollection<ExchangePackage>("ExchangePackages");
        }
        public async Task<ExchangePackage> GetLastPackage(CancellationToken cancellationToken = default)
        {
            var mostRecentPackage = await packagesCollection
                .Find(Builders<ExchangePackage>.Filter.Gte("time", DateTime.UtcNow.AddHours(-24)))
                .Sort(Builders<ExchangePackage>.Sort.Descending("time"))
                .Limit(1)
                .FirstOrDefaultAsync();

            if (mostRecentPackage != null)
            {
                return mostRecentPackage;
            }

            throw new InvalidOperationException("GetLastPackage failed. Check 'ExchangePackages 'collection");
        }

        public async Task<ExchangePackage> GetLastPairRate(PairRequest request, CancellationToken cancellationToken = default)
        {

            if (request == null || string.IsNullOrEmpty(request?.from) || string.IsNullOrEmpty(request?.to))
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");
            }

            var mostRecentPairRate = await packagesCollection
                .Find(Builders<ExchangePackage>.Filter.ElemMatch(
                    p => p.rates,
                    r => r.from == request.from && r.to == request.to))
                .Sort(Builders<ExchangePackage>.Sort.Descending("time"))
                .Limit(1)
                .FirstOrDefaultAsync();

            if (mostRecentPairRate != null)
            {
                return mostRecentPairRate;
            }

            throw new InvalidOperationException("GetLastPairRate failed.");
        }
    }
}