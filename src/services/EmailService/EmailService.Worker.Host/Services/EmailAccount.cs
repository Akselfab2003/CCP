using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService.Worker.Host.Services
{
    public class EmailAccount
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
