namespace WeatherStation.Services;
using WeatherStation.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class SensorService
{
    private readonly IMongoCollection<Sensor> _sensorsCollection;
    private readonly ILogger _logger;

    public SensorService(
        IOptions<SensorDatabaseSettings> weatherStationDatabaseSettings,
        ILogger<SensorService> logger)
    {
        _logger = logger;
        _logger.LogInformation("SensorService started");
        var mongoClient = new MongoClient(
            weatherStationDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            weatherStationDatabaseSettings.Value.DatabaseName);

        _sensorsCollection = mongoDatabase.GetCollection<Sensor>(
            weatherStationDatabaseSettings.Value.SensorsCollectionName);
    }

    public async Task<List<Sensor>> GetAsync() =>
        await _sensorsCollection.Find(_ => true).ToListAsync();

    public async Task<Sensor?> GetAsync(string id) =>
        await _sensorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public async Task<Sensor?> GetMostRecentSensorByNumberAsync(int sensorNumber)
    {
        if (sensorNumber < 1 || sensorNumber > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(sensorNumber), "Sensor number must be between 1 and 16.");
        }

        return await _sensorsCollection
            .Find(sensor => sensor.SensorNumber == sensorNumber)
            .SortByDescending(sensor => sensor.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Sensor newSensor) {
        newSensor.CreatedAt = DateTime.UtcNow;
        await _sensorsCollection.InsertOneAsync(newSensor);
    }

    public async Task UpdateAsync(string id, Sensor updatedSensor)
    {
        var existingSensor = await _sensorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (existingSensor != null)
        {
            // Set the updated properties without changing CreatedAt
            updatedSensor.CreatedAt = existingSensor.CreatedAt; // Keep the original CreatedAt value
            await _sensorsCollection.ReplaceOneAsync(x => x.Id == id, updatedSensor);
        }
    }
    public async Task RemoveAsync(string id) =>
        await _sensorsCollection.DeleteOneAsync(x => x.Id == id);
}