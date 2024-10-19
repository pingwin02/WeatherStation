using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WeatherStationBackend.Configuration;
using WeatherStationBackend.Models;

namespace WeatherStationBackend.Services;

public class RabbitMqService : IDisposable
{
    private readonly int _awardAmount;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly string _dataQueueName;
    private readonly DataService _dataService;
    private readonly ILogger _logger;
    private readonly TokenService _tokenService;
    private readonly string _transactionQueueName;

    public RabbitMqService(
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
        DataService dataService,
        TokenService tokenService,
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

        _dataQueueName = rabbitMqConfiguration.Value.DataQueueName;
        _transactionQueueName = rabbitMqConfiguration.Value.TransactionQueueName;

        _awardAmount = int.Parse(rabbitMqConfiguration.Value.AwardAmount);

        _dataService = dataService;
        _tokenService = tokenService;

        _channel.QueueDeclare(
            _dataQueueName,
            false,
            false,
            false,
            null);

        _channel.QueueDeclare(
            _transactionQueueName,
            false,
            false,
            false,
            null);

        StartReceivingMessages();
        StartReceivingTransactions();
    }

    public void Dispose()
    {
        _channel.Close();
    }

    private void StartReceivingMessages()
    {
        EventingBasicConsumer dataConsumer = new(_channel);
        dataConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                _logger.LogInformation($"Received message: {message}");
                var data = BsonSerializer.Deserialize<DataEntity>(message);
                await _dataService.AddAsync(data);
                QueueTransaction(new Transaction
                {
                    SensorId = data.SensorId,
                    Amount = _awardAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing data message");
            }
        };

        _channel.BasicConsume(
            _dataQueueName,
            true,
            dataConsumer
        );
    }

    private void StartReceivingTransactions()
    {
        EventingBasicConsumer transactionConsumer = new(_channel);

        _channel.BasicQos(0, 1, false);
        transactionConsumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                _logger.LogInformation($"Received transaction message: {message}");
                var transaction = BsonSerializer.Deserialize<Transaction>(message);
                if (transaction.SensorId == null) throw new Exception("Sensor ID is null");
                await _tokenService.TransferTokensAsync(transaction.SensorId, transaction.Amount);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing transaction message");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            _transactionQueueName,
            false,
            transactionConsumer
        );
    }

    public void QueueTransaction(Transaction transaction)
    {
        try
        {
            var message = JsonSerializer.Serialize(transaction);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                "",
                _transactionQueueName,
                null,
                body);

            _logger.LogInformation($"Queued transaction for sensor {transaction.SensorId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while queuing transaction");
        }
    }
}