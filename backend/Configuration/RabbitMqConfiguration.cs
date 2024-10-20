namespace backend.Configuration;

public class RabbitMqConfiguration
{
    public string HostName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DataQueueName { get; set; } = null!;
    public string TransactionQueueName { get; set; } = null!;
    public string AwardAmount { get; set; } = null!;
}