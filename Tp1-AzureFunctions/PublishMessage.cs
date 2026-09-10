using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TpAzureFunctions.Models;

namespace TpAzureFunctions;

public class PublishMessage
{
    private readonly ILogger<PublishMessage> _logger;

    public PublishMessage(ILogger<PublishMessage> logger)
    {
        _logger = logger;
    }

    [Function("PublishMessage")]
    [QueueOutput("tp-messages", Connection = "AzureWebJobsStorage")]
    public QueuePayload Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "messages")] MessageRequest request)
    {
        _logger.LogInformation(
            "Requête HTTP reçue pour {Name}",
            request.Name);

        var message = new QueuePayload
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = request.Name,
            Content = request.Content,
            ReceivedAtUtc = DateTimeOffset.UtcNow
        };

        _logger.LogInformation(
            "Publication du message {Id} dans la queue tp-messages",
            message.Id);

        return message;
    }
}