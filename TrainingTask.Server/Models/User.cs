using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TrainingTask.Server.Models
{
    public class User
    {
        [BsonRequired]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement("username")]
        public string Username { get; set; }

        [BsonRequired]
        [BsonElement("password")]
        public string Password { get; set; }
    }
}