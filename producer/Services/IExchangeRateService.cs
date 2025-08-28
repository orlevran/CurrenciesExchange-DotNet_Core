using producer.Models;
using producer.Models.DTOs;

namespace producer.Services
{
    public interface IExchangeRateService
    {
        Task<ExchangePackage> DocumentPackage(ExchangePackageRequest request, CancellationToken cancellationToken = default);
    }
}