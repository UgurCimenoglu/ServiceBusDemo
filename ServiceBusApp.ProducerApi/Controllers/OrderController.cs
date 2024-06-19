using Azure.Messaging.ServiceBus.Administration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using ServiceBusApp.Common;
using ServiceBusApp.Common.Dto;
using ServiceBusApp.Common.Events;
using ServiceBusApp.ProducerApi.Services;

namespace ServiceBusApp.ProducerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AzureService _azureService;

        public OrderController(AzureService azureService)
        {
            _azureService = azureService;
        }

        [HttpPost]
        public async Task OrderCreate(OrderCreateDto data)
        {
            //insert order into db

            var orderCreatedEvent = new OrderCreatedEvent()
            {
                Id = data.Id,
                CreatedOn = DateTime.Now,
                ProductName = data.Name
            };

            await _azureService.CreateQueueIfNotExist(Constants.OrderCreatedQueueName);
            await _azureService.SendMessageToQueueOrTopic(Constants.OrderCreatedQueueName, orderCreatedEvent);
        }

        [HttpDelete("{id}")]
        public async Task DeleteOrder(int id)
        {
            //delete order into db

            var orderDeletedEvent = new OrderDeletedEvent()
            {
                Id = id,
                CreatedOn = DateTime.Now
            };

            await _azureService.CreateQueueIfNotExist(Constants.OrderDeletedQueueName);
            await _azureService.SendMessageToQueueOrTopic(Constants.OrderDeletedQueueName, orderDeletedEvent);

        }

        [HttpPost("[action]")]
        public async Task OrderCreateTopic(OrderCreateDto data)
        {

            var orderCreatedEvent = new OrderCreatedEvent()
            {
                Id = data.Id,
                CreatedOn = DateTime.Now,
                ProductName = data.Name
            };

            await _azureService.CreateTopicIfNotExist(Constants.OrderCreatedTopic);
            await _azureService.CreateSubscriptionWithFilterIfNotExist(Constants.OrderCreatedTopic, Constants.OrderCreatedSub, new SqlRuleFilter("id > 100"));
            await _azureService.SendMessageToQueueOrTopic(Constants.OrderCreatedTopic, orderCreatedEvent);
        }

        [HttpPost("[action]")]
        public async Task OrderDeleteTopic(OrderDeletedEvent data)
        {

            var orderDeletedEvent = new OrderDeletedEvent()
            {
                Id = data.Id,
                CreatedOn = DateTime.Now
            };

            await _azureService.CreateTopicIfNotExist(Constants.OrderDeletedTopic);
            await _azureService.CreateSubscriptionWithFilterIfNotExist(Constants.OrderDeletedTopic, Constants.OrderDeletedSub);
            await _azureService.SendMessageToQueueOrTopic(Constants.OrderDeletedTopic, orderDeletedEvent);
        }
    }
}
