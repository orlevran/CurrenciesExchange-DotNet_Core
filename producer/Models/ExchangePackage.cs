using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace producer.Models
{
    // This file defines the ExchangePackage class, which represents a package of currency exchange rates.
    // It is designed to be stored in MongoDB, with an ObjectId as its unique identifier.
    // The class contains a timestamp (time) and a list of ExchangeRate objects (rates).
    // The constructor initializes the timestamp to the current UTC time and creates an empty list of rates.
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