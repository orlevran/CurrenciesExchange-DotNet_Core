namespace producer.Configurations
{
    public class ExchangeProviderConfig
    {
        public required string Provider { get; set; }
        public required string Key { get; set; }
        public required bool Active { get; set; }
    }
}