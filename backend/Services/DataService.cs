using System.Globalization;
using System.Text.Json;
using backend.Configuration;
using backend.Controllers;
using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace backend.Services;

public class DataService
{
    private readonly IMongoCollection<DataEntity> _dataCollection;
    private readonly SensorService _sensorService;

    public DataService(
        IOptions<DatabaseSettings> databaseSettings,
        ILogger<DataService> logger,
        SensorService sensorService)
    {
        logger.LogInformation("DataService started");

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

        if (!string.IsNullOrEmpty(sensorId)) query = query.Where(data => data.SensorId == sensorId);

        if (startDate.HasValue)
            query = query.Where(data => data.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(data => data.Timestamp <= endDate.Value);

        if (!string.IsNullOrEmpty(sortBy))
            query = sortBy switch
            {
                "value" => sortOrder == "desc"
                    ? query.OrderByDescending(data => data.Value)
                    : query.OrderBy(data => data.Value),
                "timestamp" => sortOrder == "desc"
                    ? query.OrderByDescending(data => data.Timestamp)
                    : query.OrderBy(data => data.Timestamp),
                _ => throw new ArgumentException("Invalid sortBy field. Please use 'value' or 'timestamp'.")
            };
        else if (!string.IsNullOrEmpty(sortOrder))
            throw new ArgumentException("Sort order requires a sortBy field. Please use 'value' or 'timestamp'.");

        if (int.TryParse(limit, out var limitValue) && limitValue > 0) query = query.Take(limitValue);

        return await query.ToListAsync();
    }


    public async Task<string> ExportToCsvAsync(List<DataEntity> data)
    {
        var csvFilePath = Path.Combine("Exports", $"data_{DateTime.Now:yyyyMMddHHmmss}.csv");

        using (var writer = new StreamWriter(csvFilePath))
        {
            await writer.WriteLineAsync("Id,SensorId,Value,Timestamp");
            foreach (var item in data)
                await writer.WriteLineAsync($"{item.Id}," +
                                            $"{item.SensorId}," +
                                            $"{item.Value.ToString(CultureInfo.InvariantCulture)}," +
                                            $"{item.Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}");
        }

        return csvFilePath;
    }

    public async Task<string> ExportToJsonAsync(List<DataEntity> data)
    {
        var jsonFilePath = Path.Combine("Exports", $"data_{DateTime.Now:yyyyMMddHHmmss}.json");

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(jsonFilePath, json);

        return jsonFilePath;
    }

    public async Task AddAsync(DataEntity newData)
    {
        await _dataCollection.InsertOneAsync(newData);
        await WebSocketController.SendMessage(JsonSerializer.Serialize(newData));
    }

    public async Task DeleteBySensorIdAsync(string sensorId)
    {
        await _dataCollection.DeleteManyAsync(d => d.SensorId == sensorId);
    }
}