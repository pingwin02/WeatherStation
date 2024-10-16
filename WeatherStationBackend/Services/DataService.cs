using WeatherStationBackend.Configuration;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.Web3.Accounts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using WeatherStationBackend.Controllers;

namespace WeatherStationBackend.Services;

using Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class DataService
{
    private readonly IMongoCollection<DataEntity> _dataCollection;
    private readonly ILogger _logger;
    private readonly TokenService _tokenService;
    private readonly SensorService _sensorService;

    public DataService(
        IOptions<DatabaseSettings> databaseSettings,
        ILogger<DataService> logger,
        TokenService tokenService,
        SensorService sensorService
        )
    {
        _logger = logger;
        _logger.LogInformation("DataService started");
        _tokenService = tokenService;
        _sensorService = sensorService;

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
        await SendTokensToSensor(newData.SensorId);
        _logger.LogInformation($"Added new data for SensorId: {newData.SensorId}");
    }
    
    public async Task DeleteBySensorIdAsync(string sensorId)
    {
        await _dataCollection.DeleteManyAsync(d => d.SensorId == sensorId);
        _logger.LogInformation($"Deleted all data for SensorId: {sensorId}");
    }
    
    private async Task SendTokensToSensor(string sensorId)
    {
        const int amount = 100;
        var sensor = await _sensorService.GetAsync(sensorId);
        var success = await _tokenService.TransferTokensAsync(sensor.TokenAddress, amount);

        if (success)
        {
            _logger.LogInformation($"Successfully sent tokens to sensor {sensorId}");
        }
        else
        {
            _logger.LogError($"Failed to send tokens to sensor {sensorId}");
        }
    }
}