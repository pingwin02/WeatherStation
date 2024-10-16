using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace WeatherStationBackend.Models;

public class SensorRequest
{
    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [BsonElement("type")]
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [BsonElement("token_address")]
    [JsonPropertyName("token_address")]
    public string? TokenAddress { get; set; }
}