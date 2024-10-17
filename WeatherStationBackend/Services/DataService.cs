using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WeatherStationBackend.Configuration;
using WeatherStationBackend.Models;

namespace WeatherStationBackend.Services;

public class DataService
{
    private readonly IMongoCollection<DataEntity> _dataCollection;
    private readonly ILogger _logger;
    private readonly SensorService _sensorService;
    private readonly TokenService _tokenService;

    public DataService(
        IOptions<DatabaseSettings> databaseSettings,
        ILogger<DataService> logger,
        SensorService sensorService,
        TokenService tokenService)
    {
        _logger = logger;
        _logger.LogInformation("DataService started");

        var mongoClient = new MongoClient(
            databaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            databaseSettings.Value.DatabaseName);

        _dataCollection = mongoDatabase.GetCollection<DataEntity>(
            databaseSettings.Value.DataCollectionName);

        _sensorService = sensorService;
        _tokenService = tokenService;
    }

    public async Task<List<DataEntity>> GetAllAsync()
    {
        return await _dataCollection.Find(_ => true).ToListAsync();
    }

    public async Task<List<DataEntity>> GetBySensorIdAsync(string sensorId)
    {
        return await _dataCollection.Find(d => d.SensorId == sensorId).ToListAsync();
    }

    public async Task<DataEntity?> GetMostRecentBySensorIdAsync(string sensorId)
    {
        return await _dataCollection.Find(d => d.SensorId == sensorId)
            .SortByDescending(d => d.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(DataEntity newData)
    {
        await _dataCollection.InsertOneAsync(newData);

        var sensorAdress = _sensorService.GetAsync(newData.SensorId).Result.WalletAddress;

        var result = await _tokenService.TransferTokensAsync(sensorAdress);

        if (!result)
            _logger.LogError($"Failed to transfer tokens to {sensorAdress}");
    }

    public async Task DeleteBySensorIdAsync(string sensorId)
    {
        await _dataCollection.DeleteManyAsync(d => d.SensorId == sensorId);
    }
}