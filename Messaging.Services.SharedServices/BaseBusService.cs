using MassTransit;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Services.SharedServices
{
    public class BaseBusService
    {
        private readonly IBusControl _busControl;
        public BaseBusService(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _busControl.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _busControl.StopAsync(cancellationToken);
        }
    }
}
