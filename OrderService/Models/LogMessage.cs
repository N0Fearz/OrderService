namespace OrderService.Models;

public class LogMessage
{
    public string ServiceName { get; set; }
    public string LogLevel { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}