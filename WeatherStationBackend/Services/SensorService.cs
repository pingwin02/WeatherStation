using MongoDB.Bson;
using WeatherStationBackend.Configuration;

namespace WeatherStationBackend.Services;
using Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class SensorService
{
    private readonly IMongoCollection<SensorEntity> _sensorsCollection;
    private readonly ILogger _logger;

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
    
    public async Task<List<SensorEntity>> GetAsync() =>
        await _sensorsCollection.Find(_ => true).ToListAsync();

    public async Task<SensorEntity> GetAsync(string id)
    {
        var sensor = await _sensorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        
        if (sensor == null)
        {
            throw new KeyNotFoundException($"Sensor with id {id} not found.");
        }
        
        return sensor;
    }
    
    public async Task<string> CreateAsync(SensorRequest newSensor)
    {
        if (!Enum.IsDefined(typeof(SensorType), newSensor.Type))
        {
            throw new ArgumentException($"Sensor type {newSensor.Type} is not valid.");
        }
        
        var sensorToInsert = new SensorEntity
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = newSensor.Name,
            Type = newSensor.Type,
            TokenAddress = newSensor.TokenAddress,
        };
        
        await _sensorsCollection.InsertOneAsync(sensorToInsert);
        _logger.LogInformation($"Sensor {newSensor.Name} created.");
        
        return sensorToInsert.Id;
    }
    
    public async Task UpdateAsync(string id, SensorRequest updatedSensor)
    {
        if (!Enum.IsDefined(typeof(SensorType), updatedSensor.Type))
        {
            throw new ArgumentException($"Sensor type {updatedSensor.Type} is not valid.");
        }
        
        var existingSensor = await _sensorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (existingSensor != null)
        {
            var sensorToUpdate = new SensorEntity
            {
                Id = id,
                Name = updatedSensor.Name,
                Type = updatedSensor.Type,
            };
            await _sensorsCollection.ReplaceOneAsync(x => x.Id == id, sensorToUpdate);
            _logger.LogInformation($"Sensor {id} updated.");
        }
        else
        {
            throw new KeyNotFoundException($"Sensor with id {id} not found.");
        }
    }
    
    public async Task RemoveAsync(string id) {
        var sensor = await _sensorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        
        if (sensor != null)
        {
            await _sensorsCollection.DeleteOneAsync(x => x.Id == id);
        }
        else
        {
            throw new KeyNotFoundException($"Sensor with id {id} not found.");
        }
    }
        
}
