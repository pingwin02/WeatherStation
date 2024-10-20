namespace WeatherStationBackend.Configuration;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string SensorsCollectionName { get; set; } = null!;
    public string DataCollectionName { get; set; } = null!;
}