using MassTransit;
using Messaging.Services.SharedServices;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Faces.WebMvc.Services
{
    public class BusService : BaseBusService, IHostedService
    {
        public BusService(IBusControl busControl) : base(busControl)
        {

        }
    }
}
