using Microsoft.EntityFrameworkCore;
using OrdersApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersApi.Persistence.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private OrdersContext _ordersContext;

        public OrderRepository(OrdersContext ordersContext)
        {
            _ordersContext = ordersContext;
        }

        public Order GetOrder(Guid id)
        {
            return _ordersContext.Orders
               .Include("OrderDetails")
               .FirstOrDefault(i => i.OrderId == id);
        }

        public async Task<Order> GetOrderAsync(Guid id)
        {
            return await _ordersContext.Orders
                .Include("OrderDetails")
                .FirstOrDefaultAsync(i => i.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _ordersContext.Orders
                .ToListAsync();
        }

        public async Task RegisterOrder(Order order)
        {
            await _ordersContext.AddAsync(order);
            await _ordersContext.SaveChangesAsync();
        }

        public async Task UpdateOrder(Order order)
        {
            _ordersContext.Entry(order).State = EntityState.Modified;
            await _ordersContext.SaveChangesAsync();
        }
    }
}
