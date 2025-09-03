using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace consumer.Models
{
    public class ExchangePackage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; private set; }

        [BsonElement("time")]
        [JsonPropertyName("time")]
        public DateTime time { get; set; }

        [BsonElement("rates")]
        [JsonPropertyName("rates")]
        public List<ExchangeRate> rates { get; set; } = new();
    }
}