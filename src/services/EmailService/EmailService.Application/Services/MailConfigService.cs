using CCP.Encryption;
using CCP.Shared.ResultAbstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmailService.Application.Services
{
    public class MailConfigService
    {
        private readonly string _configPath;
        private readonly ILogger<MailConfigService> _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public MailConfigService(ILogger<MailConfigService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configPath = configuration.GetValue<string>("MailConfigPath") ?? throw new ArgumentNullException("MailConfigPath is not configured.");
        }


        public async Task<Result> CreateMailBoxAsync(string Email, string password)
        {
            await _lock.WaitAsync();
            try
            {
                var passwordHash = BcryptHashing.HashPassword(password);
                var line = $"{Email}|{{BLF-CRYPT}}{passwordHash}\n";
                var path = Path.Combine(_configPath, "postfix-accounts.cf");
                var existingLines = File.Exists(path)
                                        ? await File.ReadAllLinesAsync(path)
                                        : [];
                if (existingLines.Contains(Email))
                    return Result.Failure(Error.Conflict(code: "MailboxAlreadyExists", description: $"A mailbox for {Email} already exists."));

                await File.AppendAllTextAsync(path, line);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create mailbox for {Email}", Email);
            }
            finally
            {
                _lock.Release();
            }
            return Result.Failure(Error.Failure(code: "MailboxCreationFailed", description: $"Failed to create mailbox for {Email}"));

        }
    }
}
