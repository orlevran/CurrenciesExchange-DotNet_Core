using producer.Configurations;
using Confluent.Kafka;
using System.Text.Json;

namespace producer.Services
{
    public sealed class KafkaProducerService : IKafkaProducerService
    {
        private readonly KafkaSettings settings;
        private readonly IProducer<string, string> producer;

        private readonly ILogger<KafkaProducerService> logger;
        private readonly JsonSerializerOptions jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> _log)
        {
            settings = config.GetSection("Kafka").Get<KafkaSettings>() ?? new();
            var pconf = new ProducerConfig
            {
                BootstrapServers = settings.BootstrapServers,
                Acks = Acks.All,
                LingerMs = 5,
                // Optional WAN-friendly settings:
                CompressionType = CompressionType.Lz4,  // or Snappy/Zstd
                MessageTimeoutMs = 30000,                // 30s send timeout
                RetryBackoffMs = 200,                    // backoff between retries
                EnableIdempotence = true                 // safe re-tries
            };
            producer = new ProducerBuilder<string, string>(pconf).Build();
            logger = _log;
        }
        public async Task ProduceMessageAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("KafkaProducerService.ProduceMessageAsync : Start producing {msg}", message?.ToString() ?? "(null)");

            var json = JsonSerializer.Serialize(message, jsonOpts);
            var key = Guid.NewGuid().ToString("N");

            var msg = new Message<string, string>
            {
                Key = key,
                Value = json,
                Headers = new Headers
                {
                    // new Header("schema", "exchangepackage-v1"u8.ToArray())
                }
            };

            var dr = await producer.ProduceAsync(settings.Topic, msg, cancellationToken);

            logger.LogInformation("KafkaProducerService.ProduceMessageAsync : Produced → {TPO}", dr.TopicPartitionOffset);
        }
        
        public void Dispose()
        {
            try
            {
                // Ensure buffered messages are sent before shutdown
                producer.Flush(TimeSpan.FromSeconds(5));
            }
            catch { /* swallow on shutdown */ }
            producer.Dispose();
        }
    }
}