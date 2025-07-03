using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TrainingTask.Server.Models;

namespace TrainingTask.Server.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IConfiguration configuration, ILogger<MongoDbContext> logger)
        {
            try
            {
                var connectionString = configuration["MongoDB:ConnectionString"];
                var databaseName = configuration["MongoDB:DatabaseName"];
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(databaseName);
                _logger = logger;
            }
            catch (Exception ex)
            {
                // Optionally log the error here if you have a logger
                _logger.LogError(ex, "Error initializing MongoDbContext");
                throw new InvalidOperationException("Failed to initialize MongoDbContext. See inner exception for details.", ex);
            }
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<BotConfiguration> BotConfigurations => _database.GetCollection<BotConfiguration>("botConfigurations");
    }
}
