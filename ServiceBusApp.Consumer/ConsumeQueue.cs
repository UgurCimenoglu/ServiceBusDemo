using Azure.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceBusApp.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ServiceBusApp.Consumer
{
    public class ConsumeQueue : IHostedService
    {
        private readonly string _queueName;
        private readonly ILogger<ConsumeQueue> _logger;

        private readonly ServiceBusClient _client;
        private ServiceBusProcessor _processor;
        public ConsumeQueue(string queueName, ILogger<ConsumeQueue> logger)
        {
            _queueName = queueName;
            _logger = logger;

            _client = new ServiceBusClient(Constants.ConnectionString, new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            });

            _processor = _client.CreateProcessor(_queueName, new ServiceBusProcessorOptions()
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false // Mesajları manuel tamamlayacağız
            });

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service Bus Queue Listener is starting.");
            await _processor.StartProcessingAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Service Bus Queue Listener is stopping.");

            // İşlemeyi durdur
            if (_processor != null)
            {
                await _processor.StopProcessingAsync(cancellationToken);
                await _processor.DisposeAsync();
            }

            await _client.DisposeAsync();
        }

        public async Task MessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                // Mesajı işleme
                string body = args.Message.Body.ToString();
                Console.WriteLine($"received message: {body}");

                // Mesajı başarılı bir şekilde işledikten sonra tamamla
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Message processing failed: {ex.Message}");
                // Hata olduğunda mesajı tamamlamayın, kuyrukta kalacak ve tekrar işlenecek
            }
        }

        public Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(JsonSerializer.Serialize(args));
            return Task.CompletedTask;
        }
    }
}