using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceBusApp.Common;
using ServiceBusApp.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceBusApp.Consumer
{
    public class ConsumeSub<T> : IHostedService
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusProcessor _processor;

        public ConsumeSub(string topicName, string subName)
        {
            _client = new ServiceBusClient(Constants.ConnectionString);
            _processor = _client.CreateProcessor(topicName, subName, new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 1,
            });

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting the message processor.");

            await _processor.StartProcessingAsync(cancellationToken);

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);

            await _processor.DisposeAsync();
            await _client.DisposeAsync();

        }

        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            if (typeof(T) == typeof(OrderCreatedEvent))
            {
                Console.WriteLine("\n\n OrderCreatedEvent triggered \n");
            }
            else
            {
                Console.WriteLine("\n\n OrderDeletedEvent triggered \n");
            }

            Console.WriteLine("Received a message with ID: {0}", args.Message.MessageId);
            try
            {
                foreach (var prop in args.Message.ApplicationProperties)
                {
                    Console.WriteLine("Key = {0} - Value = {1}", prop.Key, prop.Value);
                }
                Console.WriteLine(JsonSerializer.Deserialize<object>(args.Message.Body));

                // Mesajı başarılı bir şekilde işledikten sonra tamamlamak için
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex with : {ex}");
                await args.DeadLetterMessageAsync(args.Message);

            }

        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine("Error handling message: {0}", args.Exception.Message);
            return Task.CompletedTask;
        }
    }
}
