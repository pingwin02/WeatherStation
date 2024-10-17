using System.Text;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WeatherStationBackend.Configuration;
using WeatherStationBackend.Models;

namespace WeatherStationBackend.Services;

public class RabbitMqService
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly DataService _dataService;
    private readonly ILogger _logger;
    private readonly string _queueName;


    public RabbitMqService(
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
        DataService dataService,
        ILogger<RabbitMqService> logger)
    {
        _logger = logger;
        _logger.LogInformation("RabbitMqService started");
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqConfiguration.Value.HostName,
            UserName = rabbitMqConfiguration.Value.Username,
            Password = rabbitMqConfiguration.Value.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _queueName = rabbitMqConfiguration.Value.QueueName;
        _dataService = dataService;

        _channel.QueueDeclare(
            _queueName,
            false,
            false,
            false,
            null);
        StartReceivingMessages();
    }

    private void StartReceivingMessages()
    {
        EventingBasicConsumer consumer = new(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var data = BsonSerializer.Deserialize<DataEntity>(message);
                await _dataService.AddAsync(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing message");
            }
        };

        _channel.BasicConsume(
            _queueName,
            true,
            consumer
        );
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}