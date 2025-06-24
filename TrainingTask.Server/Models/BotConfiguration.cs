using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TrainingTask.Server.Models
{
    public class BotConfiguration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string BotName { get; set; }
        public string ProjectId { get; set; }
        public string LanguageCode { get; set; }

        [BsonIgnoreIfNull]
        public string CredentialsJson { get; set; }

        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}


