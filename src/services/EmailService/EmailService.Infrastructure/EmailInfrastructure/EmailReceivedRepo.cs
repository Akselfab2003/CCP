using System;
using System.Collections.Generic;
using System.Text;
using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using EmailService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class EmailReceivedRepo : IEmailReceived
    {
        private readonly DBcontext _dbContext;

        public EmailReceivedRepo(DBcontext dBcontext)
        {
            _dbContext = dBcontext;
        }

        public async Task<EmailReceived?> GetByIdAsync(int id)
        {
            return await _dbContext.EmailReceived.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EmailReceived?> GetByOrganizationIdAsync(Guid organizationId)
        {
            return await _dbContext.EmailReceived.FirstOrDefaultAsync(e => e.OrganizationId == organizationId);
        }

        public async Task<Result> CreateAsync(EmailReceived email)
        {
            if (email == null)
            {
                return Result.Failure(Error.NullValue);
            }
            email.ReceivedAt = DateTime.UtcNow;
            _dbContext.EmailReceived.Add(email);
            await _dbContext.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var emailReceived = await _dbContext.EmailReceived.FirstOrDefaultAsync(e => e.Id == id);
            if (emailReceived == null)
            {
                return Result.Failure(Error.NotFound("EmailReceived.NotFound", $"Email with ID {id} not found"));
            }
            _dbContext.EmailReceived.Remove(emailReceived);
            await _dbContext.SaveChangesAsync();
            return Result.Success();
        }
    }
}
