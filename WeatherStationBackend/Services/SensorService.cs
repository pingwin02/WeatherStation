using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using WeatherStationBackend.Configuration;
using WeatherStationBackend.Models;

namespace WeatherStationBackend.Services;

public class SensorService
{
    private readonly ILogger _logger;
    private readonly IMongoCollection<SensorEntity> _sensorsCollection;

    public SensorService(
        IOptions<DatabaseSettings> databaseSettings,
        ILogger<SensorService> logger)
    {
        _logger = logger;
        _logger.LogInformation("SensorService started");

        var mongoClient = new MongoClient(
            databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            databaseSettings.Value.DatabaseName);

        _sensorsCollection = mongoDatabase.GetCollection<SensorEntity>(
            databaseSettings.Value.SensorsCollectionName);
    }

    public async Task<List<SensorEntity>> GetAsync()
    {
        return await _sensorsCollection.Find(_ => true).ToListAsync();
    }

    public async Task<SensorEntity> GetAsync(string id)
    {
        var sensor = await _sensorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (sensor == null) throw new KeyNotFoundException($"Sensor with id {id} not found.");

        return sensor;
    }

    public async Task<List<SensorEntity>> GetByTypeAsync(string type)
    {
        return await _sensorsCollection.Find(x => x.Type == type).ToListAsync();
    }


    public async Task<List<SensorEntity>> GetByNameAsync(string name)
    {
        return await _sensorsCollection.Find(x => x.Name == name).ToListAsync();
    }

    public async Task<string> CreateAsync(SensorRequest newSensor)
    {
        if (!Enum.IsDefined(typeof(SensorType), newSensor.Type))
            throw new ArgumentException($"Sensor type {newSensor.Type} is not valid.");

        var sensorToInsert = new SensorEntity
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = newSensor.Name,
            Type = newSensor.Type,
            WalletAddress = newSensor.WalletAddress
        };

        await _sensorsCollection.InsertOneAsync(sensorToInsert);
        _logger.LogInformation($"Sensor {newSensor.Name} created.");

        return sensorToInsert.Id;
    }

    public async Task UpdateAsync(string id, SensorRequest updatedSensor)
    {
        if (!Enum.IsDefined(typeof(SensorType), updatedSensor.Type))
            throw new ArgumentException($"Sensor type {updatedSensor.Type} is not valid.");

        var existingSensor = await _sensorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (existingSensor != null)
        {
            var sensorToUpdate = new SensorEntity
            {
                Id = id,
                Name = updatedSensor.Name,
                Type = updatedSensor.Type
            };
            await _sensorsCollection.ReplaceOneAsync(x => x.Id == id, sensorToUpdate);
            _logger.LogInformation($"Sensor {id} updated.");
        }
        else
        {
            throw new KeyNotFoundException($"Sensor with id {id} not found.");
        }
    }

    public async Task RemoveAsync(string id)
    {
        var sensor = await _sensorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (sensor != null)
        {
            await _sensorsCollection.DeleteOneAsync(x => x.Id == id);
            _logger.LogInformation($"Sensor {id} removed.");
        }
        else
        {
            throw new KeyNotFoundException($"Sensor with id {id} not found.");
        }
    }
}