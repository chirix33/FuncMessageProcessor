using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Services
{
    internal class CosmosUtilsService : ICosmosUtilsService
    {
        private readonly ILogger<CosmosUtilsService> _logger;
        private readonly string _cosmosDbEndpoint;
        private readonly string _cosmosDbName;
        private readonly string _processedMessageContainerName;

        private readonly CosmosClient _cosmosClient;

        public CosmosUtilsService(ILogger<CosmosUtilsService> logger)
        {
            _logger = logger;

            // Fetch environment variables
            _cosmosDbEndpoint = Environment.GetEnvironmentVariable("COSMOS_DB__accountEndpoint") ?? throw new ArgumentNullException("COSMOS_DB__accountEndpoint");
            _cosmosDbName = Environment.GetEnvironmentVariable("COSMOS_DB_NAME") ?? throw new ArgumentNullException("COSMOS_DB_NAME");
            _processedMessageContainerName = Environment.GetEnvironmentVariable("PROCESSED_MESSAGE_CONTAINER") ?? throw new ArgumentNullException("PROCESSED_MESSAGE_CONTAINER");

            // Initialize CosmosClient using DefaultAzureCredential
            _cosmosClient = new CosmosClient(_cosmosDbEndpoint, new DefaultAzureCredential());
        }

        public string? GetParentId(string messageFromAddress, string messageSubject)
        {
            try
            {
                return GetParentMessages(messageFromAddress, messageSubject);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching parent ID: {ex.Message}");
                return null;
            }
        }

        public string? GetParentMessages(string messageFromAddress, string messageSubject)
        {
            try
            {
                if (string.IsNullOrEmpty(messageFromAddress) || string.IsNullOrEmpty(messageSubject))
                    throw new ArgumentException("Both messageFromAddress and messageSubject must be provided.");

                // Get container client
                var container = _cosmosClient.GetDatabase(_cosmosDbName).GetContainer(_processedMessageContainerName);

                // Query Cosmos DB
                var query = new QueryDefinition(
                    "SELECT TOP 1 c.id FROM c WHERE c['from'].emailAddress.address = @message_from_address AND c['subject'] = @message_subject"
                )
                .WithParameter("@message_from_address", messageFromAddress)
                .WithParameter("@message_subject", messageSubject);

                using var feedIterator = container.GetItemQueryIterator<dynamic>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = null, // Use null if partition key is not needed
                    MaxItemCount = 1
                });

                // Process query result
                if (feedIterator.HasMoreResults)
                {
                    var response = feedIterator.ReadNextAsync().Result;
                    var parentMessage = response.Resource.FirstOrDefault();
                    return parentMessage?.id;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching parent messages: {ex.Message}");
                return null;
            }
        }

        public string? GetMessageBodyById(string messageId)
        {
            try
            {
                if (string.IsNullOrEmpty(messageId))
                    throw new ArgumentException("messageId must be provided.");

                // Get container client
                var container = _cosmosClient.GetDatabase(_cosmosDbName).GetContainer(_processedMessageContainerName);

                // Query Cosmos DB
                var query = new QueryDefinition(
                    "SELECT c.id, c['unique_body'].content FROM c WHERE c.id = @message_id"
                )
                .WithParameter("@message_id", messageId);

                using var feedIterator = container.GetItemQueryIterator<dynamic>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = null, // Use null if partition key is not needed
                    MaxItemCount = 1
                });

                // Process query result
                if (feedIterator.HasMoreResults)
                {
                    var response = feedIterator.ReadNextAsync().Result;
                    var message = response.Resource.FirstOrDefault();
                    return message?.unique_body?.content;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching message body: {ex.Message}");
                return null;
            }
        }

        public string GetLastThreeParentMessages(string messageId)
        {
            try
            {
                if (string.IsNullOrEmpty(messageId))
                    throw new ArgumentException("messageId must be provided.");

                var container = _cosmosClient.GetDatabase(_cosmosDbName).GetContainer(_processedMessageContainerName);

                var parentMessages = new List<object>();
                string? currentMessageId = messageId;

                // Fetch up to 3 parent messages
                for (int i = 1; i <= 3; i++)
                {
                    if (string.IsNullOrEmpty(currentMessageId))
                    {
                        parentMessages.Add(new { Index = i.ToString(), Content = (string?)null });
                        continue;
                    }

                    var query = new QueryDefinition(
                        "SELECT c.id, c['uniqueBody'].content, c.parentId FROM c WHERE c.id = @message_id"
                    )
                    .WithParameter("@message_id", currentMessageId);

                    using var feedIterator = container.GetItemQueryIterator<dynamic>(query, requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = null
                    });

                    if (feedIterator.HasMoreResults)
                    {
                        var response = feedIterator.ReadNextAsync().Result;
                        var message = response.Resource.FirstOrDefault();

                        if (message != null & (message.parentId != currentMessageId))
                        {
                            parentMessages.Add(new
                            {
                                Index = i.ToString(),
                                Content = new
                                {
                                    Id = message.id,
                                    Content = message.unique_body?.content
                                }
                            });

                            currentMessageId = message.parentId; // Move to the next parent message
                        }
                        else
                        {
                            parentMessages.Add(new { Index = i.ToString(), Content = (string?)null });
                            currentMessageId = null;
                        }
                    }
                    else
                    {
                        parentMessages.Add(new { Index = i.ToString(), Content = (string?)null });
                        currentMessageId = null;
                    }
                }

                // Prepare the JSON result
                var result = new
                {
                    MessageId = messageId,
                    ParentMessages = parentMessages
                };
                _logger.LogInformation($"Last three parent messages: {JsonSerializer.Serialize(result)}");
                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching last three parent messages: {ex.Message}");
                return JsonSerializer.Serialize(new
                {
                    MessageId = messageId,
                    ParentMessages = new List<object>
                {
                    new { Index = "1", Content = (string?)null },
                    new { Index = "2", Content = (string?)null },
                    new { Index = "3", Content = (string?)null }
                }
                });
            }
        }
    }
}
