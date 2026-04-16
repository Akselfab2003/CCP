using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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





    }
}
