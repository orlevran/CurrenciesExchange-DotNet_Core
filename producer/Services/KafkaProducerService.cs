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

        // JSON serialization options
        private readonly JsonSerializerOptions jsonOpts = new()
        {
            // Configure JSON serialization options:
            // CamelCase property names, case-insensitive deserialization
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Case insensitive when reading JSON
            WriteIndented = false
        };
        public KafkaProducerService(IConfiguration config, ILogger<KafkaProducerService> _log)
        {
            settings = config.GetSection("Kafka").Get<KafkaSettings>() ?? new();

            /*
            Create Kafka producer with appropriate settings:
            - BootstrapServers: Kafka broker addresses
            - Acks: Wait for all replicas to acknowledge
            - LingerMs: Wait up to 5ms to batch messages
            - CompressionType: Compress messages to save bandwidth
            - MessageTimeoutMs: 30s send timeout
            - RetryBackoffMs: Backoff between retries in ms
            - EnableIdempotence: Ensure exactly-once delivery
            */
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

        /// <summary>
        /// Produce a message to Kafka topic
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        public async Task ProduceMessageAsync<T>(T message, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("KafkaProducerService.ProduceMessageAsync : Start producing {msg}", message?.ToString() ?? "(null)");

            var json = JsonSerializer.Serialize(message, jsonOpts);
            // Use a GUID as the message key to ensure uniqueness.
            // This helps with message ordering and partitioning in Kafka.
            var key = Guid.NewGuid().ToString("N");

            /*
             Create the Kafka message with key, value, and optional headers
             Use a GUID as the message key to ensure uniqueness.
             This helps with message ordering and partitioning in Kafka.
            */
            var msg = new Message<string, string>
            {
                Key = key,
                Value = json,
                Headers = new Headers
                {
                    // new Header("schema", "exchangepackage-v1"u8.ToArray())
                }
            };

            // Produce the message to the configured topic
            // This is an async call that returns when the message is acknowledged
            var dr = await producer.ProduceAsync(settings.Topic, msg, cancellationToken);

            logger.LogInformation("KafkaProducerService.ProduceMessageAsync : Produced → {TPO}", dr.TopicPartitionOffset);
        }
        
        /// <summary>
        /// Dispose the Kafka producer
        /// </summary>
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