using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Repository
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetOrders();
        Order GetOrderById(int id);
        void DeleteOrderById(int id);
        void UpdateOrder(Order order);
        void InsertOrder(Order order);
        void Save();
    }
}
