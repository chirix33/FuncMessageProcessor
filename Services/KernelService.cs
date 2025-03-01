using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Services
{
    public class KernelService : IKernelService
    {
        private string _serviceId = Environment.GetEnvironmentVariable("DEFAULT_SERVICE_ID")!;

        // OpenAI 
        private static readonly string _openAIModelId = Environment.GetEnvironmentVariable("OPENAI_MODEL")!;
        private static readonly string _openAIApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
        private static readonly string _openAiEndpoint = Environment.GetEnvironmentVariable("OPENAI_API_ENDPOINT")!;

        // Deep Seek
        private readonly string _dsModelId = Environment.GetEnvironmentVariable("DEEPSEEK_MODEL")!;
        private readonly string _dsApiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")!;
        private readonly string _dsAiEndpoint = Environment.GetEnvironmentVariable("DEEPSEEK_API_ENDPOINT")!;

        // Kernel Builder Defaults (OpenAI)
        private string model = "";
        private string apiKey = "";
        private string endpoint = "";

        private readonly string _embeddingModel = Environment.GetEnvironmentVariable("EMBEDDING_MODEL")!;

        public KernelService()
        {
            switch (_serviceId)
            {
                case "openai":
                    model = _openAIModelId;
                    apiKey = _openAIApiKey;
                    endpoint = _openAiEndpoint;
                    break;
                case "deepseek":
                    model = _dsModelId;
                    apiKey = _dsApiKey;
                    endpoint = _dsAiEndpoint;
                    break;
                default:
                    model = _openAIModelId;
                    apiKey = _openAIApiKey;
                    endpoint = _openAiEndpoint;
                    break;

            }
        }

        public ServiceProvider BuildServiceProvider(string? serviceId = null)
        {
            if (serviceId != null)
            {
                _serviceId = serviceId;
            }

            if (_serviceId == "openai")
            {
                model = _openAIModelId;
                apiKey = _openAIApiKey;
                endpoint = _openAiEndpoint;
            }

            #pragma warning disable SKEXP0010
            var services = new ServiceCollection();
            var kernelBuilder = services.AddKernel();
            services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
            kernelBuilder.AddOpenAIChatCompletion(
                modelId: model,
                endpoint: new Uri(endpoint),
                apiKey: apiKey);
            return services.BuildServiceProvider();
        }
    }
}
