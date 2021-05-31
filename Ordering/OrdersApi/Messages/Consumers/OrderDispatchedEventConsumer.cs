using MassTransit;
using Messaging.Sharedlib.Events;
using Microsoft.AspNetCore.SignalR;
using OrdersApi.Hubs;
using OrdersApi.Persistence.Repository;
using System;
using System.Threading.Tasks;

namespace OrdersApi.Messages.Consumers
{
    public class OrderDispatchedEventConsumer : IConsumer<IOrderDispatchedEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderDispatchedEventConsumer(IOrderRepository orderRepository, IHubContext<OrderHub> hubContext)
        {
            _orderRepository = orderRepository;
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<IOrderDispatchedEvent> context)
        {
            var message = context.Message;
            Guid orderId = message.OrderId;
            await UpdateDatabase(orderId);
            await _hubContext.Clients.All.SendAsync("UpdateOrders", new object[] { "Order Dispatched", orderId });
        }

        private async Task UpdateDatabase(Guid orderId)
        {
            var order = _orderRepository.GetOrder(orderId);
            if (order != null)
            {
                order.Status = Models.Status.Sent;
                await _orderRepository.UpdateOrder(order);
            }
        }
    }
}
