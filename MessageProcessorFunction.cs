using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net;
using FuncMessageProcessor.Models;
using FuncMessageProcessor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
namespace FuncMessageProcessor
{
    [StorageAccount("STORAGE_ACCOUNT")]
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
        [BlobOutput("%PROCESSED_MESSAGE_CONTAINER%/{filename}")]
        public IActionResult Run(
        [BlobTrigger("%MESSAGE_CONTAINER%/{filename}")] EnrichedMessage blobMessage)
        {
            _logger.LogInformation("C# Blob trigger function processed a request.");

            if (blobMessage == null)
            {
                return new BadRequestObjectResult("Please pass a message in the request body");
            }

            _messageProcessor.ProcessMessage(blobMessage);

            return new OkObjectResult(JsonSerializer.Serialize(blobMessage));
        }
    }
}
