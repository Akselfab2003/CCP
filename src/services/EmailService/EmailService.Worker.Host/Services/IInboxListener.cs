using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService.Worker.Host.Services
{
    public interface IInboxListener
    {
        Task ListenSingleInboxAsync(EmailAccount account, CancellationToken cancellationToken);
        Task ListenToAllInboxesAsync(IEnumerable<EmailAccount> accounts, CancellationToken cancellationToken);
    }
}
