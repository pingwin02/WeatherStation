using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WeatherStation.Models;

public class Sensor
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Name")]
    [JsonPropertyName("Name")]
    public string SensorName { get; set; } = null!;

    public int SensorNumber { get; set; }
    
    public double SensorValue { get; set; }
    
    [BsonElement("created_at")]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; private set; }
    public Sensor()
    {
        CreatedAt = DateTime.UtcNow;
    }
}