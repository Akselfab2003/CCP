using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using EmailService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class TenantEmailConfigurationRepo : ITenantEmailConfigurationRepo, IEmailWorkerConfigurationRepo
    {
        private readonly ILogger<TenantEmailConfigurationRepo> _logger;
        private readonly DBcontext _dbContext;

        public TenantEmailConfigurationRepo(ILogger<TenantEmailConfigurationRepo> logger, DBcontext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<Result> AddAsync(TenantEmailConfiguration tenantEmailConfiguration)
        {
            try
            {
                await _dbContext.TenantEmailConfigurations.AddAsync(tenantEmailConfiguration);
                await _dbContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding tenant email configurat0ion.");
                return Result.Failure(Error.Failure(code: "AddTenantEmailConfigurationFailed",
                                                    description: "Failed to add tenant email configuration."));
            }
        }


        public async Task<Result> UpdateAsync(TenantEmailConfiguration tenantEmailConfiguration)
        {
            try
            {
                _dbContext.TenantEmailConfigurations.Update(tenantEmailConfiguration);
                await _dbContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating tenant email configuration.");
                return Result.Failure(Error.Failure(code: "UpdateTenantEmailConfigurationFailed",
                                                    description: "Failed to update tenant email configuration."));
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                var tenantEmailConfiguration = await _dbContext.TenantEmailConfigurations.FindAsync(id);
                if (tenantEmailConfiguration == null)
                {
                    return Result.Failure(Error.NotFound("TenantEmailConfiguration.NotFound", $"Tenant email configuration with ID {id} not found"));
                }
                _dbContext.TenantEmailConfigurations.Remove(tenantEmailConfiguration);
                await _dbContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting tenant email configuration.");
                return Result.Failure(Error.Failure(code: "DeleteTenantEmailConfigurationFailed",
                                                    description: "Failed to delete tenant email configuration."));
            }
        }

        public async Task<Result<TenantEmailConfiguration>> GetByIdAsync(Guid id)
        {
            try
            {
                var val = await _dbContext.TenantEmailConfigurations.SingleOrDefaultAsync(t => t.Id == id);

                return val is null
                    ? Result.Failure<TenantEmailConfiguration>(Error.NotFound("TenantEmailConfiguration.NotFound", $"Tenant email configuration with ID {id} not found"))
                    : Result.Success(val);
            }
            catch (Exception)
            {
                return Result.Failure<TenantEmailConfiguration>(Error.Failure(code: "GetTenantEmailConfigurationFailed",
                                                                              description: "Failed to retrieve tenant email configuration."));
            }
        }


        public async Task<Result<List<TenantEmailConfiguration>>> GetAllAsync()
        {
            try
            {
                var AllTenantEmailConfigurations = await _dbContext.TenantEmailConfigurations.IgnoreQueryFilters()
                                                                                             .ToListAsync();

                return AllTenantEmailConfigurations is null
                    ? Result.Failure<List<TenantEmailConfiguration>>(Error.NotFound("TenantEmailConfigurations.NotFound", "No tenant email configurations found"))
                    : Result.Success(AllTenantEmailConfigurations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tenant email configurations.");
                return Result.Failure<List<TenantEmailConfiguration>>(Error.Failure(code: "GetAllTenantEmailConfigurationsFailed",
                                                                              description: "Failed to retrieve tenant email configurations."));
            }
        }
    }
}
