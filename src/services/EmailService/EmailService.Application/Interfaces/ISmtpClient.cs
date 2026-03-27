using System;
using System.Collections.Generic;
using System.Text;
using MimeKit;

namespace EmailService.Application.Interfaces
{
    public interface ISmtpClient
    {
        Task SendAsync(MimeMessage message);
    }
}
