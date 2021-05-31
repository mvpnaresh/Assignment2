using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Sharedlib.Events
{
    public interface IOrderDispatchedEvent
    {
        public Guid OrderId { get; set; }
        public DateTime DispatchedDateTime { get; set; }
    }
}
