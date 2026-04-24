using Microsoft.Extensions.Logging;

namespace CCP.Shared.AuthContext
{
    public class TenantHeaderInjector : DelegatingHandler
    {
        private readonly ServiceAccountOverrider _currentUser;
        private readonly ILogger<TenantHeaderInjector> _logger;
        public TenantHeaderInjector(ServiceAccountOverrider currentUser, ILogger<TenantHeaderInjector> logger)
        {
            _currentUser = currentUser;
            _logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.OrganizationId;
            if (tenantId != Guid.Empty)
            {
                request.Headers.Remove("X-Tenant-ID");
                request.Headers.Add("X-Tenant-ID", tenantId.ToString());
                _logger.LogInformation("Added X-Tenant-ID header with value {TenantId} to outgoing request.", tenantId);
            }
            else
            {
                _logger.LogWarning("No tenant ID found in current user context. X-Tenant-ID header will not be added to outgoing request.");
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
