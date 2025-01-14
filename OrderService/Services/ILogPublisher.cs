using OrderService.Models;

namespace OrderService.Services;

public interface ILogPublisher
{
    void SendMessage(LogMessage message);
}