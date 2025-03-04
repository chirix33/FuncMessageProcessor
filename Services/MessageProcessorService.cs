using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnsClient.Internal;
using FuncMessageProcessor.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Catalyst;
using Catalyst.Models;
using Mosaik.Core;
using System.Text.Json;
using FuncMessageProcessor.Services;
using Microsoft.Graph.Models;

namespace FuncMessageProcessor.Services
{
    public class MessageProcessorService : IMessageProcessor
    {
        private readonly ILogger<MessageProcessorService> _logger;


        public MessageProcessorService(ILogger<MessageProcessorService> logger)
        {
            _logger = logger;
        }

        public void ProcessMessage(EnrichedMessage inputMessage)
        {
            if (String.IsNullOrEmpty(inputMessage.Body.Content))
            {
                throw new ArgumentException("Input message cannot be null or empty");
            }

            string body = inputMessage.Body.Content;

            if (inputMessage.UniqueBody == null)
            {
                var cleanedHTML = Utils.HtmlCleaner(body);

                inputMessage.UniqueBody = new ItemBody()
                {
                    ContentType = BodyType.Html,
                    Content = cleanedHTML
                };

                _logger.LogInformation("Unique body extracted from the input message");
            }

            // Extract text only content from the input message's HTML and set it to 
            // the AiData.Cleaned property
            string textOnlyContent = Utils.HtmlToText(inputMessage.UniqueBody.Content);

            
            inputMessage.AiData.Cleaned = textOnlyContent;
            
            _logger.LogInformation("Done cleaning the message");
        }
    }
}
