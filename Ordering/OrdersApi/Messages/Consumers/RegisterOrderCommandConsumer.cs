using MassTransit;
using Messaging.Sharedlib.Commands;
using Messaging.Sharedlib.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrdersApi.Hubs;
using OrdersApi.Models;
using OrdersApi.Persistence.Repository;
using OrdersApi.Settings;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrdersApi.Messages.Consumers
{
    public class RegisterOrderCommandConsumer : IConsumer<IRegisterOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly IOptions<OrderSettings> _orderSettings;

        public RegisterOrderCommandConsumer(IOrderRepository orderRepository, IHttpClientFactory httpClientFactory
            , IHubContext<OrderHub> hubContext, IConfiguration configuration
            , IOptions<OrderSettings> orderSettings)
        {
            _orderRepository = orderRepository;
            _clientFactory = httpClientFactory;
            _hubContext = hubContext;
            _orderSettings = orderSettings;
        }

        public async Task Consume(ConsumeContext<IRegisterOrderCommand> context)
        {
            var result = context.Message;
            if (result.OrderId != Guid.Empty
                && result.PictureUrl != null
                && result.UserEmail != null
                && result.ImageData != null)
            {
                await SaveOrder(result);

                await _hubContext.Clients.All.SendAsync("UpdateOrders", new object[] { "New Order Created", result.OrderId });

                await PublishOrderProcessedEvent(context);

            }
        }

        public async Task PublishOrderProcessedEvent(ConsumeContext<IRegisterOrderCommand> context)
        {
            var result = context.Message;
            var client = _clientFactory.CreateClient();

            Tuple<List<byte[]>, Guid> orderDetailData = await GetFacesFromFaceApiAsync(client, result.ImageData, result.OrderId);
            List<byte[]> faces = orderDetailData.Item1;
            Guid orderId = orderDetailData.Item2;

            await SaveOrderDetails(orderId, faces);

            await _hubContext.Clients.All.SendAsync("UpdateOrders", new object[] { "Order Processed", result.OrderId });

            await context.Publish<IOrderProcessedEvent>(new
            {
                OrderId = orderId,
                result.UserEmail,
                Faces = faces,
                result.PictureUrl
            });
        }

        private async Task<Tuple<List<byte[]>, Guid>> GetFacesFromFaceApiAsync(HttpClient client, byte[] imageData, Guid orderId)
        {
            var byteContent = new ByteArrayContent(imageData);
            Tuple<List<byte[]>, Guid> orderDetailData = null;

            var faceApiUrl = _orderSettings.Value.FacesApiUrl + "?orderId=";

            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            
            using (var response = await client.PostAsync(faceApiUrl + orderId, byteContent))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                orderDetailData = JsonConvert.DeserializeObject<Tuple<List<byte[]>, Guid>>(apiResponse);
            }
            return orderDetailData;
        }

        private async Task SaveOrderDetails(Guid orderId, List<byte[]> faces)
        {
            var order = await _orderRepository.GetOrderAsync(orderId);
            if (order != null)
            {
                order.Status = Status.Processed;
                foreach (var face in faces)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = orderId,
                        FaceData = face
                    };
                    order.OrderDetails.Add(orderDetail);
                }
                await _orderRepository.UpdateOrder(order);
            }
        }

        private async Task SaveOrder(IRegisterOrderCommand result)
        {
            Order order = new Order()
            {
                OrderId = result.OrderId,
                UserEmail = result.UserEmail,
                Status = Status.Registered,
                PictureUrl = result.PictureUrl,
                ImageData = result.ImageData
            };
            await _orderRepository.RegisterOrder(order);
        }
    }
}
