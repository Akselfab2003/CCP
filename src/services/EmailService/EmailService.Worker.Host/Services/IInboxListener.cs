using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService.Worker.Host.Services
{
    public interface IInboxListener
    {
        Task ListenAsync(CancellationToken cancellationToken);
    }
}
