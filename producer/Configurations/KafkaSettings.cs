namespace producer.Configurations
{
    public sealed class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string Topic { get; set; } = "exchange-packages";
    }
}