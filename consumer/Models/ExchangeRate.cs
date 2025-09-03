using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace consumer.Models
{
    public class ExchangeRate
    {
        //[BsonElement("from")]
        [BsonElement("from")]
        [JsonPropertyName("from")]
        public string? from { get; set; }

        [BsonElement("to")]
        [JsonPropertyName("to")]
        public string? to { get; set; }

        [BsonElement("rate")]
        [JsonPropertyName("rate")]
        public decimal? rate { get; set; }
    }
}