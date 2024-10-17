namespace WeatherStationBackend.Services;

public class RabbitMqBackgroundService : BackgroundService
{
    private readonly RabbitMqService _rabbitMqService;

    public RabbitMqBackgroundService(RabbitMqService rabbitMqService)
    {
        _rabbitMqService = rabbitMqService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _rabbitMqService.Dispose();
        base.Dispose();
    }
}