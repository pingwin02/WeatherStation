using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WeatherStation.Models;

public class Sensor
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string SensorName { get; set; } = null!;
    
    [BsonElement("number")]
    [JsonPropertyName("number")]
    public int SensorNumber { get; set; }
    
    [BsonElement("value")]
    [JsonPropertyName("value")]
    public double SensorValue { get; set; }
    
    [BsonElement("created_at")]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}