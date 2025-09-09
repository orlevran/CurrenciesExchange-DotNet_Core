namespace producer.Models.DTOs
{
    public class ExchangePackageRequest
    {
        // List of currencies to exchange in real time: 1 Key = X Value
        public required List<Tuple<string, string>> currencies { get; set; }
    }
}