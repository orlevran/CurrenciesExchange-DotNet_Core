using consumer.Models;
using consumer.Models.DTOs;

namespace consumer.Services
{
    public interface IConsumerService
    {
        Task<ExchangePackage> GetLastPackage(CancellationToken cancellationToken = default);
        Task<ExchangePackage> GetLastPairRate(PairRequest request, CancellationToken cancellationToken = default);
    }
}