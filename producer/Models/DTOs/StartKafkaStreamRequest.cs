namespace producer.Models.DTOs
{
    /// <summary>
    /// Represents a currency pair with source and target currencies.
    /// </summary>
    /// <param name="From"></param>
    /// <param name="To"></param>
    public record PairDto(string From, string To);

    /// <summary>
    /// Represents a request to start a Kafka stream with specified currency pairs and interval.
    /// </summary>
    public class StartKafkaStreamRequest
    {
        public required List<PairDto> Pairs { get; init; }
        public int IntervalInSeconds { get; init; } = 60;
    }
}