namespace TpAzureFunctions.Models;

public class QueuePayload
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAtUtc { get; set; }
}