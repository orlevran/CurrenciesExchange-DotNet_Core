using consumer.Models;
using consumer.Models.DTOs;

using MongoDB.Driver;

namespace consumer.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly IMongoCollection<ExchangePackage> packagesCollection;

        /// <summary>
        /// Initializes a new instance of ConsumerService.
        /// Sets up access to the "ExchangePackages" collection in MongoDB.
        /// </summary>
        /// <param name="database">The MongoDB database instance.</param>
        public ConsumerService(IMongoDatabase database)
        {
            packagesCollection = database.GetCollection<ExchangePackage>("ExchangePackages");
        }

        /// <summary> 
        /// Retrieves the most recent ExchangePackage/> 
        /// from the last 24 hours, based on the "time" field.
        /// </summary>
        ///  cancellationToken: Optional cancellation token for async operation.
        /// <returns>The most recent ExchangePackage in the last 24 hours.</returns>
        /// InvalidOperationException: Thrown when no recent package is found in the "ExchangePackages" collection.
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

        /// <summary>
        /// Retrieves the most recent <see cref="ExchangePackage"/> 
        /// that contains an exchange rate for the specified currency pair.
        /// </summary>
        /// request: The currency pair request containing 'from' and 'to' values.
        /// cancellationToken: Optional cancellation token for async operation.
        /// Returns: The most recent ExchangePackage that includes the requested currency pair.
        /// ArgumentNullException :Thrown if the request is null or has missing values.
        /// InvalidOperationException :Thrown if no package is found for the specified currency pair.
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