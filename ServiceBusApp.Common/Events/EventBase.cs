using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusApp.Common.Events
{
    public class EventBase
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
