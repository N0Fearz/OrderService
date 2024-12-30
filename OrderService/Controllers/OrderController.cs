using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Repository;
using OrderService.Services;

namespace OrderService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderController(IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _httpContextAccessor = httpContextAccessor;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var accessToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", string.Empty);
            var schemaName = await _orderService.GetTenantSchemaName(accessToken);
            _orderService.SetConnectionString(schemaName);
            
            var orders = _orderRepository.GetOrders();
            return Ok(orders);
        }
    }
}
