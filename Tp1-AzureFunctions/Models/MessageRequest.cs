namespace TpAzureFunctions.Models;

public class MessageRequest
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}