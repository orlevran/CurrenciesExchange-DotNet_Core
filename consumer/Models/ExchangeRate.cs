using MongoDB.Bson.Serialization.Attributes;

namespace consumer.Models
{
    public class ExchangeRate
    {
        //[BsonElement("from")]
        [BsonElement("from")]
        public string? from { get; set; }

        [BsonElement("to")]
        public string? to { get; set; }

        [BsonElement("rate")]
        public decimal? rate { get; set; }

        public ExchangeRate(string _from, string _to, decimal _rate)
        {
            from = _from;
            to = _to;
            rate = _rate;
        }
    }
}