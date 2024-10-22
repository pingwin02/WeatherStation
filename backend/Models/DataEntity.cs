using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models;

public class DataEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [BsonElement("sensorId")]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("sensorId")]
    public string? SensorId { get; set; }

    [BsonElement("value")]
    [JsonPropertyName("value")]
    public double Value { get; set; }

    [BsonElement("timestamp")]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}