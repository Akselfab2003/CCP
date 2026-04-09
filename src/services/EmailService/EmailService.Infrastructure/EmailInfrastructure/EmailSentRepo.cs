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
    public class EmailSentRepo : IEmailSent
    {
        private readonly DBcontext _dbContext;

        public EmailSentRepo(DBcontext dBcontext)
        {
            _dbContext = dBcontext;
        }

        public async Task<EmailSent?> GetByIdAsync(int id)
        {
            return await _dbContext.EmailSent.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EmailSent?> GetByOrganizationIdAsync(Guid organizationId)
        {
            return await _dbContext.EmailSent.FirstOrDefaultAsync(e => e.OrganizationId == organizationId);
        }

        public async Task<Result> CreateAsync(EmailSent email)
        {
            if (email == null)
            {
                return Result.Failure(Error.NullValue);
            }
            email.SentAt = DateTime.UtcNow;
            _dbContext.EmailSent.Add(email);
            await _dbContext.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var emailSent = await _dbContext.EmailSent.FirstOrDefaultAsync(e => e.Id == id);

            if (emailSent == null)
            {
                return Result.Failure(Error.NotFound("EmailSent.NotFound", $"Email with ID {id} not found"));
            }

            _dbContext.EmailSent.Remove(emailSent);
            await _dbContext.SaveChangesAsync();

            return Result.Success();
        }
    }
}
