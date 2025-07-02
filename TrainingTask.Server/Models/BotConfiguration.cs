using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TrainingTask.Server.Models
{
    public class BotConfiguration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        public string BotName { get; set; }

        public string JsonCreds { get; set; }

        public string LanguageCode { get; set; }
    }
}


