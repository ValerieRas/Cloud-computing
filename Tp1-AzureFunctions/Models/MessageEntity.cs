namespace TpAzureFunctions.Models;

public class MessageEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public DateTimeOffset ReceivedAtUtc { get; set; }
}