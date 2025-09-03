using consumer.Configurations;
using consumer.Models;
using Confluent.Kafka;
using System.Text.Json;
using Confluent.Kafka.Admin;

namespace consumer.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly KafkaSettings settings;
        private readonly IConfiguration config;
        private readonly ILastPackageCache cache;
        private readonly ILogger<KafkaConsumerService> log;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
        public KafkaConsumerService(IConfiguration _config, ILastPackageCache _cache, ILogger<KafkaConsumerService> _log)
        {
            config = _config;
            settings = config.GetSection("Kafka").Get<KafkaSettings>() ?? new();
            cache = _cache;
            log = _log;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mode = config["RunMode"] ?? "Kafka";

            if (!string.Equals(mode, "Kafka", StringComparison.OrdinalIgnoreCase))
            {
                log.LogInformation("RunMode={Mode}; KafkaConsumerService will not start.", mode);
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.BootstrapServers))
            {
                log.LogError("Kafka BootstrapServers not configured.");
                return;
            }
            if (string.IsNullOrWhiteSpace(settings.Topic))
            {
                log.LogError("Kafka Topic not configured.");
                return;
            }

            // 1) Ensure topic exists (dev-friendly; idempotent)
            try
            {
                using var admin = new AdminClientBuilder(new AdminClientConfig
                {
                    BootstrapServers = settings.BootstrapServers
                }).Build();

                await admin.CreateTopicsAsync(new[]
                {
                    new TopicSpecification
                    {
                        Name = settings.Topic,
                        NumPartitions = 3,
                        ReplicationFactor = 1
                    }
                });

                log.LogInformation("Created topic '{Topic}'.", settings.Topic);
            }
            catch (CreateTopicsException e) when (e.Results.Count > 0 &&
                                          e.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
            {
                log.LogInformation("Topic '{Topic}' already exists.", settings.Topic);
            }
            catch (Exception ex)
            {
                // Non-fatal for dev; you can decide to return/throw in prod.
                log.LogWarning(ex, "Topic ensure failed; will proceed and retry if needed.");
            }

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = settings.BootstrapServers,
                GroupId = string.IsNullOrWhiteSpace(settings.GroupId) ? "ce-consumer-1" : settings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) => log.LogError("Kafka error: {Reason}", e.Reason))
                .Build();

            consumer.Subscribe(settings.Topic);
            log.LogInformation("Kafka consumer subscribed to topic '{Topic}'", settings.Topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (cr is null) continue;

                    // Handle tombstones / null values gracefully
                    if (cr.Message?.Value is null)
                    {
                        log.LogWarning("Null message at {TPO}", cr.TopicPartitionOffset);
                        continue;
                    }

                    try
                    {
                        var pkg = JsonSerializer.Deserialize<ExchangePackage>(cr.Message.Value, JsonOpts);
                        if (pkg == null)
                        {
                            log.LogWarning("Null/invalid payload at {TPO}", cr.TopicPartitionOffset);
                            continue;
                        }

                        cache.SetLastPackage(pkg);

                        log.LogInformation("Consumed {TPO} time={Time} rates={Count}",
                            cr.TopicPartitionOffset, pkg.time, pkg.rates?.Count ?? 0);
                    }
                    catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        // Topic not ready yet on the broker; retry with backoff.
                        log.LogWarning("Topic '{Topic}' not ready. Retrying in 5s...", settings.Topic);
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        consumer.Subscribe(settings.Topic); // re-subscribe just in case
                    }
                    catch (JsonException jex)
                    {
                        log.LogError(jex, "JSON deserialization error: {Message}", jex.Message);
                        log.LogError(jex, "JSON deserialization error. Payload: {Payload}", cr.Message.Value);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        // graceful shutdown
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Consume loop error: {Message}", ex.Message);
                        log.LogError(ex, "Consume loop error. Payload: {Payload}", cr.Message.Value);
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    }
                }
            }
            finally
            {
                consumer.Close();
                log.LogInformation("Kafka consumer closed.");
            }
        }
    }
}