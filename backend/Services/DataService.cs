using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using WeatherStationBackend.Configuration;
using WeatherStationBackend.Models;

namespace WeatherStationBackend.Services;

public class DataService
{
    private readonly IMongoCollection<DataEntity> _dataCollection;
    private readonly ILogger _logger;
    private readonly SensorService _sensorService;

    public DataService(
        IOptions<DatabaseSettings> databaseSettings,
        ILogger<DataService> logger,
        SensorService sensorService)
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
    }

    public async Task<List<DataEntity>> GetFilteredDataAsync(
        string? sensorType,
        string? sensorId,
        string? sensorName,
        DateTime? startDate,
        DateTime? endDate,
        string? limit,
        string? sortBy,
        string? sortOrder)
    {
        var query = _dataCollection.AsQueryable();

        if (!string.IsNullOrEmpty(sensorType))
        {
            var sensorIds = await _sensorService.GetByTypeAsync(sensorType);
            query = query.Where(data => sensorIds.Select(s => s.Id).Contains(data.SensorId));
        }

        if (!string.IsNullOrEmpty(sensorId))
        {
            var sensorIds = await _sensorService.GetAsync();
            var matchingSensorIds = sensorIds
                .Where(s => s.Id!.Contains(sensorId))
                .Select(s => s.Id)
                .ToList();

            query = query.Where(data => matchingSensorIds.Contains(data.SensorId));
        }

        if (!string.IsNullOrEmpty(sensorName))
        {
            var sensorIds = await _sensorService.GetAsync();
            var matchingSensorIds = sensorIds
                .Where(s => s.Name!.Contains(sensorName))
                .Select(s => s.Id)
                .ToList();

            query = query.Where(data => matchingSensorIds.Contains(data.SensorId));
        }

        if (startDate.HasValue)
            query = query.Where(data => data.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(data => data.Timestamp <= endDate.Value);

        if (!string.IsNullOrEmpty(sortBy))
        {
            var sensors = await _sensorService.GetAsync();

            query = sortBy switch
            {
                "value" => sortOrder == "desc"
                    ? query.OrderByDescending(data => data.Value)
                    : query.OrderBy(data => data.Value),
                "timestamp" => sortOrder == "desc"
                    ? query.OrderByDescending(data => data.Timestamp)
                    : query.OrderBy(data => data.Timestamp),
                "sensorId" => sortOrder == "desc"
                    ? query.OrderByDescending(data => data.SensorId)
                    : query.OrderBy(data => data.SensorId),
                "sensorType" => sortOrder == "desc"
                    ? query.OrderByDescending(data => sensors.FirstOrDefault(s => s.Id == data.SensorId)!.Type)
                    : query.OrderBy(data => sensors.FirstOrDefault(s => s.Id == data.SensorId)!.Type),
                "sensorName" => sortOrder == "desc"
                    ? query.OrderByDescending(data => sensors.FirstOrDefault(s => s.Id == data.SensorId)!.Name)
                    : query.OrderBy(data => sensors.FirstOrDefault(s => s.Id == data.SensorId)!.Name),
                _ => throw new ArgumentException(
                    "Invalid sortBy field. Please use 'value', 'timestamp', 'sensorId', 'sensorType', or 'sensorName'.")
            };
        }
        else if (!string.IsNullOrEmpty(sortOrder))
        {
            throw new ArgumentException(
                "Sort order requires a sortBy field. Please use 'value', 'timestamp', 'sensorId', 'sensorType', or 'sensorName'.");
        }

        if (int.TryParse(limit, out var limitValue) && limitValue > 0) query = query.Take(limitValue);

        return await query.ToListAsync();
    }


    public async Task<Stream> ExportToCsvAsync(List<DataEntity> data)
    {
        var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        {
            await writer.WriteLineAsync("Id,SensorId,Value,Timestamp");
            foreach (var item in data)
                await writer.WriteLineAsync($"{item.Id}," +
                                            $"{item.SensorId}," +
                                            $"{item.Value.ToString(CultureInfo.InvariantCulture)}," +
                                            $"{item.Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}");
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<Stream> ExportToJsonAsync(List<DataEntity> data)
    {
        var memoryStream = new MemoryStream();
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        var bytes = Encoding.UTF8.GetBytes(json);
        await memoryStream.WriteAsync(bytes, 0, bytes.Length);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task AddAsync(DataEntity newData)
    {
        await _dataCollection.InsertOneAsync(newData);
    }

    public async Task DeleteBySensorIdAsync(string sensorId)
    {
        await _dataCollection.DeleteManyAsync(d => d.SensorId == sensorId);
    }
}