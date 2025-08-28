using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace producer.Models
{
    public class ExchangePackage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; private set; }
        [BsonElement("time")]
        public DateTime time { get; set; }
        [BsonElement("rates")]
        public List<ExchangeRate> rates { get; set; }

        public ExchangePackage()
        {
            time = DateTime.UtcNow;
            rates = new List<ExchangeRate>();
        }
    }
}