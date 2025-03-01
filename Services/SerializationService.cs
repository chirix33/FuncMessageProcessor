using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Services
{
    public class SerializationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Kernel _kernel;
        private readonly ILogger<SerializationService> _logger;
        public SerializationService(IKernelService serviceProvider, ILogger<SerializationService> logger)
        {
            _serviceProvider = serviceProvider.BuildServiceProvider();
            _kernel = _serviceProvider.GetRequiredService<Kernel>();
            _logger = logger;
        }
        public async Task<T> SerializeOutputAsync<T>(
            KernelFunction kernelFunction,
            KernelArguments arguments,
            int maxRetries = 2,
            int throttleLimitSeconds = 3,
            string? message = "")
        {
            int attempts = 0;

            while (attempts < maxRetries)
            {
                try
                {
                    // Execute the KernelFunction
                    FunctionResult results = await kernelFunction.InvokeAsync(_kernel, arguments);

                    // If no results, throw an exception
                    string response = results.GetValue<string>() ?? throw new InvalidOperationException("Function returned no result.");

                    _logger.LogInformation($"Attempt {attempts + 1}: Successfully received response: {response}");

                    // Deserialize the response to ensure it matches the provided type
                    var responseObject = JsonSerializer.Deserialize<T>(response);

                    // Serialize the object back to a JSON string
                    //string jsonResponse = JsonSerializer.Serialize(responseObject);

                    // Return the JSON response
                    return responseObject;
                }
                catch (JsonException jsonEx)
                {
                    // Log and retry on JSON serialization issues
                    _logger.LogWarning(jsonEx, $"Attempt {attempts + 1}: Response was not valid JSON. Retrying...");
                }
                catch (Exception ex)
                {
                    // Log and retry on other exceptions
                    _logger.LogError(ex, $"Attempt {attempts + 1}: Error during processing. Retrying...");
                }

                // Increment attempts and throttle
                attempts++;
                if (attempts < maxRetries)
                {
                    _logger.LogInformation($"Throttling for {throttleLimitSeconds} seconds before retrying...");
                    await Task.Delay(throttleLimitSeconds * 1000);
                }
            }
            throw new InvalidOperationException("Maximum number of retries reached. Unable to process the request.");
        }

        public static string ExtractValidJson(string input)
        {
            // This regex looks for the first and last curly braces to extract valid JSON
            var match = Regex.Match(input, @"\{.*\}", RegexOptions.Singleline);
            return match.Success ? match.Value : string.Empty;
        }
    }
}
