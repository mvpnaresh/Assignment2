using EmailService;
using MassTransit;
using Messaging.Sharedlib.Events;
using SixLabors.ImageSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace NotificationService.Consumers
{
    public class OrderProcessedEventConsumer : IConsumer<IOrderProcessedEvent>
    {
        private readonly IEmailSender _emailSender;
        public OrderProcessedEventConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<IOrderProcessedEvent> context)
        {
            var rootFolder = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
            var result = context.Message;
            var facesData = result.Faces;
            if (facesData.Count < 1)
            {
                await Console.Out.WriteLineAsync($"No faces detected");
            }
            else
            {
                int j = 0;
                foreach (var face in facesData)
                {
                    MemoryStream ms = new(face);

                    var image = Image.Load(ms.ToArray());
                    image.Save(rootFolder + @"\Images\face" + j + ".jpg");
                    
                    j++;
                }
            }

            string[] mailAddress = { result.UserEmail };

            var emailMessage = new Message(mailAddress, "Your Order " + result.OrderId, "From FacesAndFaces", facesData);

            await _emailSender.SendEmailAsync(emailMessage);

            await context.Publish<IOrderDispatchedEvent>(
                new
                {
                    context.Message.OrderId,
                    DispatchDateTime = DateTime.Now
                });
        }
    }
}
