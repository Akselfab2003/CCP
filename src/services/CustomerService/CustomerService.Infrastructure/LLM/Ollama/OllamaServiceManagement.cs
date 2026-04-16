using CCP.Shared.ResultAbstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace CustomerService.Infrastructure.LLM.Ollama
{
    public class OllamaServiceManagement
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OllamaServiceManagement> _logger;
        public OllamaServiceManagement(IConfiguration configuration, ILogger<OllamaServiceManagement> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Result<IOllamaApiClient> ConfigureOllamaClient(string ModelName)
        {
            try
            {


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring Ollama client");
                return Result.Failure<IOllamaApiClient>(Error.Failure(code: "OllamaClientConfigurationError", description: "An error occurred while configuring the Ollama client."));
            }
        }



    }
}
