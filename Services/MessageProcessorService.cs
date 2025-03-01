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
        private readonly IServiceProvider _serviceProvider;
        private readonly Kernel _kernel;
        private readonly string entitesTopicsPromptTemplate = "";
        private readonly string tagsPromptTemplate = "";
        private readonly PromptTemplateService _promptTemplateService;
        private readonly SerializationService _serializationService;
        private readonly String company = Environment.GetEnvironmentVariable("COMPANY");


        public MessageProcessorService(
            ILogger<MessageProcessorService> logger,
            IKernelService serviceProvider,
            PromptTemplateService promptTemplateService,
            SerializationService serializationService,
            AppConfigModel appConfig
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider.BuildServiceProvider();
            _kernel = _serviceProvider.GetRequiredService<Kernel>();
            entitesTopicsPromptTemplate = appConfig.aidata;
            tagsPromptTemplate = appConfig.tags;
            _promptTemplateService = promptTemplateService;
            _serializationService = serializationService;
            Catalyst.Models.English.Register();
        }

        public async Task ProcessMessage(EnrichedMessage inputMessage)
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

            // Extract entities, topics, signature, and salutation from the input message
            var entitiesAndTopics = await ExtractEntitiesAndTopics(textOnlyContent);
            inputMessage.AiData = entitiesAndTopics;
            inputMessage.AiData.Cleaned = textOnlyContent;

            // Get the tags of the email body
            //var tags = await AssignTags(textOnlyContent);
            //inputMessage.AiData.Tags = tags;

            _logger.LogInformation($"Input Message: ${JsonSerializer.Serialize(inputMessage)}");

            _logger.LogInformation("Done");
        }

        public async Task<AiData> ExtractEntitiesAndTopics(String content)
        {
            // Extract entities and topics from the input message
            try
            {
                var arguments = new KernelArguments
                {
                    { "message", content },
                    { "company", company }
                };

                #pragma warning disable SKEXP0010
                OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
                {
                    ResponseFormat = typeof(AiData)
                };

                KernelFunction dcfunc = await _promptTemplateService.CreateKernelFunction(entitesTopicsPromptTemplate, arguments, "handlebars", executionSettings: settings);
                FunctionResult results = await dcfunc.InvokeAsync(_kernel, arguments);

                // If no results, use the original message
                var response = results.GetValue<string>();
                _logger.LogInformation($"Successfully received response: {response}");

                // If response is null, throw an exception
                if (String.IsNullOrEmpty(response))
                {
                    throw new Exception("Prompt template couldn't be executed");
                }

                try
                {
                    // Deserialize the response to ensure it is a valid Object
                    AiData responseObject = JsonSerializer.Deserialize<AiData>(response)!;

                    // Return the Decomposed Message object
                    return responseObject;
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Initial serialization failed. Removing illegal JSON characters...");
                }

                // If the initial serialization fails, run it throught ExtractValidJson to strip it off illegal characters
                string validJSON = SerializationService.ExtractValidJson(response);
                if (string.IsNullOrEmpty(validJSON))
                {
                    // Delegate to the SerializationService if ExtractValidJson fails
                    _logger.LogWarning("JSON output still invalid. Delegating to SerializationService for retries...");
                    return await _serializationService.SerializeOutputAsync<AiData>(dcfunc, arguments, message: response);
                }

                // Return the Decomposed Message object if it's a valid JSON
                return JsonSerializer.Deserialize<AiData>(validJSON)!;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting entities and topics from the input message: {ex}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<String> DecomposeMessage(String content)
        {
            try
            {
                var arguments = new KernelArguments
                {
                    { "message", content }
                };
                #pragma warning disable SKEXP0010
                OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
                {
                    ResponseFormat = "json_object"
                };
                KernelFunction dcfunc = await _promptTemplateService.CreateKernelFunction(entitesTopicsPromptTemplate, arguments, "handlebars", settings);
                FunctionResult results = await dcfunc.InvokeAsync(_kernel, arguments);

                return results.GetValue<string>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Tags> AssignTags(String content)
        {
            try
            {
                var arguments = new KernelArguments
                {
                    { "message", content }
                };

                #pragma warning disable SKEXP0010
                OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings()
                {
                    ResponseFormat = "json_object"
                };

                KernelFunction dcfunc = await _promptTemplateService.CreateKernelFunction(tagsPromptTemplate, arguments, "handlebars", executionSettings: settings);
                FunctionResult results = await dcfunc.InvokeAsync(_kernel, arguments);

                // If no results, use the original message
                var response = results.GetValue<string>();
                _logger.LogInformation($"Successfully received response: {response}");

                // If response is null, throw an exception
                if (String.IsNullOrEmpty(response))
                {
                    throw new Exception("Prompt template couldn't be executed");
                }

                try
                {
                    // Deserialize the response to ensure it is a valid Object
                    Tags responseObject = JsonSerializer.Deserialize<Tags>(response)!;

                    // Return the Decomposed Message object
                    return responseObject;
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Initial serialization failed. Removing illegal JSON characters...");
                }

                // If the initial serialization fails, run it throught ExtractValidJson to strip it off illegal characters
                string validJSON = SerializationService.ExtractValidJson(response);
                if (string.IsNullOrEmpty(validJSON))
                {
                    // Delegate to the SerializationService if ExtractValidJson fails
                    _logger.LogWarning("JSON output still invalid. Delegating to SerializationService for retries...");
                    return await _serializationService.SerializeOutputAsync<Tags>(dcfunc, arguments, message: response);
                }

                // Return the Decomposed Message object if it's a valid JSON
                return JsonSerializer.Deserialize<Tags>(validJSON)!;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //public async Task<String> ExtractLastNamedEntity(String content)
        //{
        //    var nlp = await Pipeline.ForAsync(Language.Any);
        //    nlp.Add(await AveragePerceptronEntityRecognizer.FromStoreAsync(language: Language.Any, version: Mosaik.Core.Version.Latest, tag: "WikiNER"));

        //    var doc = new Document(content, Language.English);
        //    nlp.ProcessSingle(doc);
        //    var entities = ReturnAllEntities(doc);
            
        //    return entities.Count > 0 ? entities[entities.Count - 1] : "";
        //}
        //public static List<String> ReturnAllEntities(IDocument doc)
        //{
        //    List<String> namedEntities = doc
        //    .SelectMany(span => span.GetEntities()) // Flatten all entities
        //    .Where(e => e.EntityType.Type == "Person") // Filter only Person entities
        //    .Select(e => e.Value) // Extract entity value
        //    .ToList(); // Convert to List
        //    return namedEntities;
        //}
    }
}
