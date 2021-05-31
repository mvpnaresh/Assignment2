﻿using OrdersApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersApi.Persistence.Repository
{
    public interface IOrderRepository
    {
        Task<Order> GetOrderAsync(Guid id);
        Task<IEnumerable<Order>> GetOrdersAsync();
        Task RegisterOrder(Order order);
        Order GetOrder(Guid id);
        Task UpdateOrder(Order order);

    }
}
