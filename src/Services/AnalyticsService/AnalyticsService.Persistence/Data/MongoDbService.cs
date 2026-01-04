using AnalyticsService.Domain.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace AnalyticsService.Persistence.Data;

public class MongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoDbService> _logger;

    public MongoDbService(IConfiguration configuration, ILogger<MongoDbService> logger)
    {
        _logger = logger;

        try
        {
            var connectionString = configuration.GetConnectionString("MongoDB");
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("AnalyticsDb");

            _logger.LogInformation("MongoDB connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MongoDB");
            throw;
        }
    }

    public IMongoCollection<T> GetCollection<T>()
    {
        var collectionName = typeof(T).GetCustomAttributes(typeof(BsonCollectionAttribute), true)
            .OfType<BsonCollectionAttribute>()
            .FirstOrDefault()?.CollectionName ?? typeof(T).Name.ToLowerInvariant();

        return _database.GetCollection<T>(collectionName);
    }
}