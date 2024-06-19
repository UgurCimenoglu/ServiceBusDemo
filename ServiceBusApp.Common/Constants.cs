using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusApp.Common
{
    public static class Constants
    {
        public const string ConnectionString = "<YourConnectionString>";

        public const string OrderCreatedQueueName = "OrderCreatedQueue";
        public const string OrderDeletedQueueName = "OrderDeletedQueue";

        public const string OrderCreatedTopic = "OrderCreatedTopic";
        public const string OrderDeletedTopic = "OrderDeletedTopic";


        public const string OrderCreatedSub = "OrderCreatedSub";
        public const string OrderDeletedSub = "OrderDeletedSub";
    }
}
