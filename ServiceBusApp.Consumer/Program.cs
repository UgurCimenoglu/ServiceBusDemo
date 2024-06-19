using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceBusApp.Common;
using ServiceBusApp.Common.Events;

namespace ServiceBusApp.Consumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
             .ConfigureServices((context, services) =>
             {
                 services.AddHostedService(provider =>
                 {
                     var logger = provider.GetRequiredService<ILogger<ConsumeQueue>>();
                     return new ConsumeQueue(Constants.OrderCreatedQueueName, logger);
                 });

                 services.AddHostedService(provider =>
                 {
                     return new ConsumeSub<OrderCreatedEvent>(Constants.OrderCreatedTopic, Constants.OrderCreatedSub);
                 });

                 services.AddHostedService(provider =>
                 {
                     return new ConsumeSub<OrderDeletedEvent>(Constants.OrderDeletedTopic, Constants.OrderDeletedSub);
                 });

             })
             .Build();

            await host.RunAsync();
        }
    }
}
