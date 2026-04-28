using CCP.Shared.AuthContext;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class TenantHeaderInjector : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantHeaderInjector> _logger;

    public TenantHeaderInjector(IHttpContextAccessor httpContextAccessor, ILogger<TenantHeaderInjector> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var overrider = _httpContextAccessor.HttpContext?.RequestServices
            .GetRequiredService<ServiceAccountOverrider>();

        var tenantId = overrider?.OrganizationId ?? Guid.Empty;

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
