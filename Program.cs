using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using Microsoft.Azure.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using FuncMessageProcessor.Services;
using System.Text.Json.Serialization;
using System.Text.Json;
using FuncMessageProcessor.Models;

namespace FuncMessageProcessor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // App Config Prefix
            string appConfigPrefix = Environment.GetEnvironmentVariable("APP_CONFIG_PREFIX") ?? "UNSET";

            var host = new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddAzureAppConfiguration(options => options.Connect(new Uri(Environment.GetEnvironmentVariable("AZURE_APP_CONFIG_ENDPOINT") ?? "UNSET"),
                        new DefaultAzureCredential())
                    .ConfigureRefresh(refresh =>
                    {
                        refresh.Register("Settings:Sentinel", refreshAll: true)
                               .SetRefreshInterval(TimeSpan.FromHours(24));
                    }));
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<AppConfigModel>(provider =>
                    {
                        var configuration = provider.GetRequiredService<IConfiguration>();

                        // 1) Create a new instance of AppConfigModel
                        var typedConfig = new AppConfigModel();

                        // 2) Bind all keys under "appConfigPrefix"
                        configuration.GetSection(appConfigPrefix).Bind(typedConfig);

                        // 3) Return the config so it's cached in DI
                        return typedConfig;
                    });
                    services.AddScoped<IMessageProcessor, MessageProcessorService>();
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.Configure<JsonSerializerOptions>(options =>
                    {
                        options.AllowTrailingCommas = true;
                        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                        options.PropertyNameCaseInsensitive = true;
                        options.WriteIndented = true;
                        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true));
                    });
                })
                .Build();

            host.Run();
        }
    }
}