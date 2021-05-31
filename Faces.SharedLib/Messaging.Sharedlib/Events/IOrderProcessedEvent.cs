using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Sharedlib.Events
{
    public interface IOrderProcessedEvent
    {
        public Guid OrderId { get; set; }
        public string PictureUrl { get; set; }
        public List<byte[]> Faces { get; set; }
        public string UserEmail { get; set; }
    }
}
