namespace WeatherStation.Services;
using WeatherStation.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class SensorService
{
    private readonly IMongoCollection<Sensor> _sensorsCollection;

    public SensorService(
        IOptions<SensorDatabaseSettings> weatherStationDatabaseSettings)
    {
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

    public async Task CreateAsync(Sensor newSensor) =>
        await _sensorsCollection.InsertOneAsync(newSensor);

    public async Task UpdateAsync(string id, Sensor updatedSensor) =>
        await _sensorsCollection.ReplaceOneAsync(x => x.Id == id, updatedSensor);

    public async Task RemoveAsync(string id) =>
        await _sensorsCollection.DeleteOneAsync(x => x.Id == id);
}