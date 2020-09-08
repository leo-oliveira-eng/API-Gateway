using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Polly;
using System;
using System.Net.Http;

namespace Api.Gateway.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPolly(this IServiceCollection services, IConfiguration configuration)
        {
            AddHttpClient(services, "HarryPotterClientConfigurationName", configuration.GetSection("HarryPotterBaseUrl").Value);
        }

        static void AddHttpClient(this IServiceCollection services, string configurationName, string baseAddress, bool retry = true)
        {
            var httpRetryPolicy = Policy.HandleResult<HttpResponseMessage>(r =>
            {
                return !r.IsSuccessStatusCode && !Convert.ToInt32(r.StatusCode).ToString().StartsWith("4");
            }).RetryAsync(3);


            if (retry)
            {
                services.AddHttpClient(configurationName, client =>
                {
                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                }).AddPolicyHandler(httpRetryPolicy);
            }
            else
            {
                services.AddHttpClient(configurationName, client =>
                {
                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });
            }
        }

        public static void AddSwaggerGen(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(x => x.FullName);
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Make Magic Challenge",
                    Contact = new OpenApiContact
                    {
                        Name = "API Gateway",
                        Email = "leo.oliveira.eng@outlook.com",
                        Url = new Uri("https://github.com/leo-oliveira-eng/API-Gateway")
                    },
                    Version = "1.0.0"
                });
            });
        }
    }
}
