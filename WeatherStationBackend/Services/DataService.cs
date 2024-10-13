using MongoDB.Bson;
using WeatherStationBackend.Configuration;

namespace WeatherStationBackend.Services;

using Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class DataService
{
    private readonly IMongoCollection<DataEntity> _dataCollection;
    private readonly ILogger _logger;

    public DataService(
        IOptions<DatabaseSettings> databaseSettings,
        ILogger<DataService> logger)
    {
        _logger = logger;
        _logger.LogInformation("DataService started");

        var mongoClient = new MongoClient(
            databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            databaseSettings.Value.DatabaseName);

        _dataCollection = mongoDatabase.GetCollection<DataEntity>(
            databaseSettings.Value.DataCollectionName);
    }

    public async Task<List<DataEntity>> GetAllAsync() =>
        await _dataCollection.Find(_ => true).ToListAsync();

    public async Task<List<DataEntity>> GetBySensorIdAsync(string sensorId) =>
        await _dataCollection.Find(d => d.SensorId == sensorId).ToListAsync();

    public async Task<DataEntity?> GetMostRecentBySensorIdAsync(string sensorId) =>
        await _dataCollection.Find(d => d.SensorId == sensorId)
            .SortByDescending(d => d.Timestamp)
            .FirstOrDefaultAsync();

    public async Task AddAsync(DataEntity newData)
    {
        await _dataCollection.InsertOneAsync(newData);
        _logger.LogInformation($"Added new data for SensorId: {newData.SensorId}");
    }
    
    public async Task DeleteBySensorIdAsync(string sensorId)
    {
        await _dataCollection.DeleteManyAsync(d => d.SensorId == sensorId);
        _logger.LogInformation($"Deleted all data for SensorId: {sensorId}");
    }
}