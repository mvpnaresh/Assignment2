using MassTransit;
using Messaging.Services.SharedServices;
using Microsoft.Extensions.Hosting;

namespace OrdersApi.Services
{
    public class BusService : BaseBusService, IHostedService
    {
        public BusService(IBusControl busControl) : base(busControl)
        {

        }

    }
}
