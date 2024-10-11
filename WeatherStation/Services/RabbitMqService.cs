using System.Text;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WeatherStation.Models;

namespace WeatherStation.Services;

public class RabbitMqService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly SensorService _sensorService;
    private readonly ILogger _logger;


    public RabbitMqService( 
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
        SensorService sensorService,
        ILogger<RabbitMqService> logger)
    {
        _logger = logger;
        _logger.LogInformation("RabbitMqService started");
        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqConfiguration.Value.HostName,
            UserName = rabbitMqConfiguration.Value.Username,
            Password = rabbitMqConfiguration.Value.Password
            
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _queueName = rabbitMqConfiguration.Value.QueueName;
        _sensorService = sensorService;

        _channel.QueueDeclare(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        StartReceivingMessages();
    }
    
    private void StartReceivingMessages()
    {
        EventingBasicConsumer consumer = new (_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            try
            {
                var sensor = BsonSerializer.Deserialize<Sensor>(message);
                await _sensorService.CreateAsync(sensor);
                Console.WriteLine("Received and saved sensor data: {0}", sensor.SensorName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        };

        _channel.BasicConsume(
            queue: _queueName, 
            autoAck: true, 
            consumer: consumer
        );
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}