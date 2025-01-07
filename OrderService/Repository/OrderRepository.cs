using OrderService.Data;
using OrderService.DTO;
using OrderService.Models;

namespace OrderService.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _orderDbContext;
        public OrderRepository(OrderDbContext orderDbContext)
        {
            this._orderDbContext = orderDbContext;
        }
        public void DeleteOrderById(int id)
        {
            var order = _orderDbContext.Orders.Find(id);
            _orderDbContext.Orders.Remove(order);
            Save();
        }

        public IEnumerable<Order> GetOrders()
        {
            return _orderDbContext.Orders.ToList();
        }

        public Order GetOrderById(int id)
        {
            return _orderDbContext.Orders.Find(id);
        }

        public void InsertOrder(Order order)
        {
            _orderDbContext.Add(order);
            Save();
        }

        public void Save()
        {
            _orderDbContext.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            _orderDbContext.Update(order);
            Save();
        }
    }
}
