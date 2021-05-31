using Faces.WebMvc.Models;
using Faces.WebMvc.ViewModels;
using MassTransit;
using Messaging.Sharedlib.Commands;
using Messaging.Sharedlib.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Faces.WebMvc.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBusControl _busControl;

        public HomeController(ILogger<HomeController> logger, IBusControl busControl)
        {
            _logger = logger;
            _busControl = busControl;
        }

        [HttpGet]
        public IActionResult RegisterOrder()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterOrder(OrderViewModel orderViewModel)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (var uploadedFile = orderViewModel.File.OpenReadStream())
            {
                await uploadedFile.CopyToAsync(memoryStream);
            }

            orderViewModel.ImageData = memoryStream.ToArray();
            orderViewModel.PictureUrl = orderViewModel.File.FileName;
            orderViewModel.OrderId = Guid.NewGuid();
            
            var sendToUri = new Uri($"{RabbitMqMassTransitConstants.RabbitMqUri}" +
                $"{ RabbitMqMassTransitConstants.RegisterOrderCommandQueue}");

            var endpoint = await _busControl.GetSendEndpoint(sendToUri);
            await endpoint.Send<IRegisterOrderCommand>(
                new
                {
                    orderViewModel.OrderId,
                    orderViewModel.UserEmail,
                    orderViewModel.ImageData,
                    orderViewModel.PictureUrl
                }
                );

            ViewData["OrderId"] = orderViewModel.OrderId;

            return View("Thanks");

        }

        public IActionResult Index()
        {
            return View();
        }

 
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
