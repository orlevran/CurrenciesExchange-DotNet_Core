using consumer.Models;

namespace consumer.Services
{
    public interface ILastPackageCache
    {
        ExchangePackage? GetLastPackage();
        void SetLastPackage(ExchangePackage package);
    }

    public class LastPackageCache : ILastPackageCache
    {
        private ExchangePackage? lastPackage;
        private readonly object lockObj = new();

        public ExchangePackage? GetLastPackage()
        {
            // Using lock to ensure thread safety
            lock (lockObj)
            {
                return lastPackage;
            }
        }

        public void SetLastPackage(ExchangePackage package)
        {
            // Using lock to ensure thread safety
            lock (lockObj)
            {
                lastPackage = package;
            }
        }
    }
}