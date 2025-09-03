namespace producer.Services
{
    public interface IKafkaProducerService
    {
        Task ProduceMessageAsync<T>(T message, CancellationToken cancellationToken = default);
    }
}