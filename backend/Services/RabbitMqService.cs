using System.Text;
using System.Text.Json;
using backend.Configuration;
using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace backend.Services;

public class RabbitMqService : IDisposable
{
    private readonly int _awardAmount;
    private readonly IModel _dataChannel;
    private readonly string _dataQueueName;
    private readonly DataService _dataService;
    private readonly ILogger _logger;
    private readonly TokenService _tokenService;
    private readonly IModel _transactionChannel;
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
        var connection = factory.CreateConnection();

        _dataChannel = connection.CreateModel();
        _transactionChannel = connection.CreateModel();
        _transactionChannel.BasicQos(0, 1, true);

        _dataQueueName = rabbitMqConfiguration.Value.DataQueueName;
        _transactionQueueName = rabbitMqConfiguration.Value.TransactionQueueName;

        _awardAmount = int.Parse(rabbitMqConfiguration.Value.AwardAmount);

        _dataService = dataService;
        _tokenService = tokenService;

        _dataChannel.QueueDeclare(
            _dataQueueName,
            false,
            false,
            false,
            null);

        _transactionChannel.QueueDeclare(
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
        _dataChannel.Close();
        _transactionChannel.Close();
    }

    private void StartReceivingMessages()
    {
        EventingBasicConsumer dataConsumer = new(_dataChannel);
        dataConsumer.Received += async (_, ea) =>
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

        _dataChannel.BasicConsume(
            _dataQueueName,
            true,
            dataConsumer
        );
    }

    private void StartReceivingTransactions()
    {
        EventingBasicConsumer transactionConsumer = new(_transactionChannel);

        transactionConsumer.Received += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                _logger.LogInformation($"Received transaction message: {message}");
                var transaction = BsonSerializer.Deserialize<Transaction>(message);
                if (transaction.SensorId == null) throw new Exception("Sensor ID is null");
                await _tokenService.TransferTokensAsync(transaction.SensorId, transaction.Amount);
                _transactionChannel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing transaction message");
                _transactionChannel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _transactionChannel.BasicConsume(
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

            _transactionChannel.BasicPublish(
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