namespace producer.Models.DTOs
{
    public record PairDto(string From, string To);
    public class StartKafkaStreamRequest
    {
        public required List<PairDto> Pairs { get; init; }
        public int IntervalInSeconds { get; init; } = 60;
    }
}