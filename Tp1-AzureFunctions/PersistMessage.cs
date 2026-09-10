using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TpAzureFunctions.Models;

namespace TpAzureFunctions;

public class PersistMessage
{
    private readonly ILogger<PersistMessage> _logger;

    public PersistMessage(ILogger<PersistMessage> logger)
    {
        _logger = logger;
    }

    [Function("PersistMessage")]
    [TableOutput("Messages", Connection = "AzureWebJobsStorage")]
    public MessageEntity Run(
        [QueueTrigger(
            "tp-messages",
            Connection = "AzureWebJobsStorage")] string queueMessage)
    {
        _logger.LogInformation(
            "Message reçu depuis la queue : {QueueMessage}",
            queueMessage);

        var message = JsonSerializer.Deserialize<QueuePayload>(
            queueMessage,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (message is null)
        {
            throw new InvalidOperationException(
                "Impossible de désérialiser le message.");
        }

        var entity = new MessageEntity
        {
            PartitionKey = "Messages",
            RowKey = message.Id,
            Name = message.Name,
            Content = message.Content,
            ReceivedAtUtc = message.ReceivedAtUtc
        };

        _logger.LogInformation(
            "Écriture du message {Id} dans Table Storage",
            message.Id);

        return entity;
    }
}