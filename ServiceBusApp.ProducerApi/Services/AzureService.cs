using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using ServiceBusApp.Common;
using ServiceBusApp.Common.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceBusApp.ProducerApi.Services
{
    public class AzureService
    {
        private readonly ServiceBusAdministrationClient _administrationClient;

        public AzureService(ServiceBusAdministrationClient administrationClient)
        {
            _administrationClient = administrationClient;
        }

        public async Task SendMessageToQueueOrTopic(string queueOrTopicName, object messageContent)
        {
            await using var client = new ServiceBusClient(Constants.ConnectionString);
            ServiceBusSender sender = client.CreateSender(queueOrTopicName);
            var serializeObject = JsonSerializer.Serialize(messageContent);
            var byteArray = Encoding.UTF8.GetBytes(serializeObject);

            ServiceBusMessage message = new ServiceBusMessage(byteArray)
            {
                ApplicationProperties = { {"id" , (messageContent as EventBase).Id }  }
            };

            await sender.SendMessageAsync(message);
        }

        public async Task CreateQueueIfNotExist(string queueName)
        {
            if (!await _administrationClient.QueueExistsAsync(queueName))
            {
                await _administrationClient.CreateQueueAsync(queueName);
            }
        }

        public async Task CreateTopicIfNotExist(string topicName)
        {
            if (!await _administrationClient.TopicExistsAsync(topicName))
            {

                await _administrationClient.CreateTopicAsync(topicName);
            }
        }

        public async Task CreateSubscriptionWithFilterIfNotExist(string topicName, string subscriptionName, RuleFilter filter = null)
        {
            if (!await _administrationClient.SubscriptionExistsAsync(topicName, subscriptionName))
            {
                await _administrationClient.CreateSubscriptionAsync(topicName, subscriptionName);

                if (filter != null)
                {
                    // Varsayılan kuralı kaldırma
                    await _administrationClient.DeleteRuleAsync(topicName, subscriptionName, RuleProperties.DefaultRuleName);

                    await _administrationClient.CreateRuleAsync(topicName, subscriptionName, new CreateRuleOptions()
                    {
                        Filter = filter,
                        Name = $"{subscriptionName}Filter"
                    });
                }
            }

        }
    }
}
