using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


namespace CCP.ServiceDefaults.Extensions
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection ConfigureDefaultOpenTelemetry(this IServiceCollection builder, string serviceName)
        {

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName);

            builder.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
                })
                .WithMetrics(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
                })
                .WithLogging(builder =>
                {
                    builder.SetResourceBuilder(resourceBuilder)
                           .AddOtlpExporter();
                });

            builder.AddHttpLogging(logging =>
            {
                logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestHeaders;
                logging.RequestHeaders.Add("X-Request-ID");
                logging.ResponseHeaders.Add("X-Response-ID");
            });

            builder.AddHealthChecks();

            return builder;
        }
    }
}
