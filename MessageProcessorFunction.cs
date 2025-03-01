using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;
using System.Net;
using FuncMessageProcessor.Models;
using FuncMessageProcessor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace FuncMessageProcessor
{
    public class MessageProcessorFunction
    {
        private readonly ILogger _logger;
        private readonly IMessageProcessor _messageProcessor;

        public MessageProcessorFunction(ILoggerFactory loggerFactory, IMessageProcessor messageProcessor)
        {
            _logger = loggerFactory.CreateLogger<MessageProcessorFunction>();
            _messageProcessor = messageProcessor;
        }

        [Function("MessageProcessor")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            [FromBody] EnrichedMessage blobMessage)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (blobMessage == null)
            {
                return new BadRequestObjectResult("Please pass a message in the request body");
            }

            await _messageProcessor.ProcessMessage(blobMessage);

            return new OkObjectResult(blobMessage);
        }
    }
}
