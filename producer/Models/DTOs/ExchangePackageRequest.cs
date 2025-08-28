namespace producer.Models.DTOs
{
    public class ExchangePackageRequest
    {
        public required List<Tuple<string, string>> currencies { get; set; }
    }
}