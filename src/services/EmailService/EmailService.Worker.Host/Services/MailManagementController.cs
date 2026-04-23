using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;
using MimeKit;

namespace EmailService.Worker.Host.Services
{
    public class MailManagementController : IMailManagementController
    {
        private readonly ILogger<MailManagementController> _logger;

        public MailManagementController(ILogger<MailManagementController> logger)
        {
            _logger = logger;
        }

        public async Task<Result<string>> GetCustomerEmailFromMail(MimeMessage message, TenantEmailConfiguration tenantEmailConfiguration)
        {
            try
            {


                return Result.Success(message.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty);

                //var internalDomain = tenantEmailConfiguration.InternalEmail.Split('@').LastOrDefault();
                //var externalDomain = tenantEmailConfiguration.DefaultSenderEmail.Split('@').LastOrDefault();
                //var from = message.From.Mailboxes.FirstOrDefault();

                //if (internalDomain == null || externalDomain == null || from == null)
                //{
                //    return Result.Failure<string>(Error.Failure(code: "InvalidInternalEmail", description: "The internal email provided in tenant configuration is invalid."));
                //}
                //bool IsForwarded = from?.Domain?.EndsWith(internalDomain, StringComparison.OrdinalIgnoreCase) == true || from?.Domain?.EndsWith(externalDomain, StringComparison.OrdinalIgnoreCase) == true;

                //if (!IsForwarded)
                //{
                //    return Result.Success(from!.Address);
                //}

                //// Fallback 1 - Try to get original sender email from "X-Forwarded-From" header added by Microsoft 365 when the email is forwarded to internal email address configured in tenant configuration.
                //var xForwardFrom = message.Headers["X-Forwarded-From"];
                //if (!string.IsNullOrEmpty(xForwardFrom))
                //{
                //    var mailboxAddress = MailboxAddress.Parse(xForwardFrom);
                //    return Result.Success(mailboxAddress.Address);
                //}

                //// Fallback 2 - Try to get original sender email from "Resent-From"
                //var resentFrom = message.ResentFrom?.Mailboxes.FirstOrDefault();
                //if (!string.IsNullOrEmpty(resentFrom?.Address))
                //{
                //    return Result.Success(resentFrom.Address);
                //}

                //// Fallback 3 - Check Reply-To
                //var replyTo = message.ReplyTo?.Mailboxes.FirstOrDefault();
                //if (!string.IsNullOrEmpty(replyTo?.Address))
                //{
                //    return Result.Success(replyTo.Address);
                //}

                //// Last resort - Parse the mail for any email not equal to internal or external domain and consider it as customer email.
                //var mailbody = message.TextBody ?? message.HtmlBody ?? string.Empty;

                //if (!string.IsNullOrEmpty(mailbody))
                //{
                //    var emailMatches = System.Text.RegularExpressions.Regex.Matches(mailbody, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
                //    foreach (System.Text.RegularExpressions.Match match in emailMatches)
                //    {
                //        var email = match.Value;
                //        if (!email.EndsWith(internalDomain, StringComparison.OrdinalIgnoreCase) && !email.EndsWith(externalDomain, StringComparison.OrdinalIgnoreCase))
                //        {
                //            return Result.Success(email);
                //        }
                //    }
                //}

                //return Result.Failure<string>(Error.Failure(code: "CustomerEmailNotFound", description: "Unable to extract customer email from the mail."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while extracting customer email from mail.");
                return Result.Failure<string>(Error.Failure(code: "EmailExtractionFailed", description: $"An error occurred while extracting customer email from mail: {ex.Message}"));
            }
        }
    }
}
