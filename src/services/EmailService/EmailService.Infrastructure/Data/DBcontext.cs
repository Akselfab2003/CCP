using System;
using System.Collections.Generic;
using System.Text;
using EmailService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailService.Infrastructure.Data
{
    public class DBcontext : DbContext
    {
        public DBcontext(DbContextOptions<DBcontext> options) : base(options)
        {
        }

        public DbSet<EmailSent> EmailSent { get; set; }
        public DbSet<EmailReceived> EmailReceived { get; set; }
    }
}
