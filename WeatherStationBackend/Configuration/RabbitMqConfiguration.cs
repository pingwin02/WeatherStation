namespace WeatherStationBackend.Configuration;

public class RabbitMqConfiguration
{
    public string HostName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string QueueName { get; set; } = null!;
}