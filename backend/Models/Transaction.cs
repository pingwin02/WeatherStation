using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace WeatherStationBackend.Models;

public class Transaction
{
    [BsonElement("sensor_id")]
    [JsonPropertyName("sensor_id")]
    public string? SensorId { get; set; }

    [BsonElement("amount")]
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
}