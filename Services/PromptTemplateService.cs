using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncMessageProcessor.Services
{
    public class PromptTemplateService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Kernel _kernel;

        public PromptTemplateService(ILogger<PromptTemplateService> logger, IKernelService serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider.BuildServiceProvider();
            _kernel = _serviceProvider.GetRequiredService<Kernel>();
        }

        public PromptTemplateConfig GetPromptTemplate(string promptTemplate, string? templateFormat = null)
        {
            if (string.IsNullOrEmpty(promptTemplate))
            {
                throw new ArgumentException($"App Configuration not found or is empty.");
            }

            _logger.LogInformation($"App configuration Found. Proceeding to next.");
            var promptTemplateConfig = new PromptTemplateConfig(promptTemplate);
            if (promptTemplateConfig != null && !String.IsNullOrEmpty(templateFormat))
            {
                promptTemplateConfig.TemplateFormat = templateFormat;
            }

            // Create and return the prompt template
            return promptTemplateConfig;
        }

        public async Task<KernelFunction> CreateKernelFunction(
            string message,
            KernelArguments arguments,
            string templateFormat,
            OpenAIPromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null)
        {
            var templateFactory = new HandlebarsPromptTemplateFactory();
            var promptTemplateConfig = GetPromptTemplate(message, templateFormat);
            var promptTemplate = templateFactory.Create(promptTemplateConfig);
            string renderedPrompt = "";

            if (kernel != null)
            {
                renderedPrompt = await promptTemplate.RenderAsync(kernel, arguments);
                return kernel.CreateFunctionFromPrompt(renderedPrompt, executionSettings);
            }

            renderedPrompt = await promptTemplate.RenderAsync(_kernel, arguments);
            return _kernel.CreateFunctionFromPrompt(renderedPrompt, executionSettings);

        }
    }
}
