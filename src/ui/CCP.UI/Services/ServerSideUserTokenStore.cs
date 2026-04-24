using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace CCP.UI.Services;

internal sealed class ServerSideUserTokenStore : IUserTokenStore
{
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStoreTokensInAuthenticationProperties _storeTokensInProps;

    public ServerSideUserTokenStore(
        IMemoryCache cache,
        IHttpContextAccessor httpContextAccessor,
        IStoreTokensInAuthenticationProperties storeTokensInProps)
    {
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
        _storeTokensInProps = storeTokensInProps;
    }

    public async Task<TokenResult<TokenForParameters>> GetTokenAsync(
        ClaimsPrincipal user,
        UserTokenRequestParameters? parameters = null,
        CancellationToken ct = default)
    {
        var key = CacheKey(user, parameters);
        if (key is null)
            return new FailedResult("Cannot resolve user sub claim for token cache key");

        if (_cache.TryGetValue(key, out UserToken? cachedToken) && cachedToken is not null)
        {
            UserRefreshToken? refreshToken = cachedToken.RefreshToken.HasValue
                ? new UserRefreshToken(cachedToken.RefreshToken.Value, DPoPProofKey: null)
                : null;

            return new TokenForParameters(cachedToken, refreshToken);
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return new FailedResult("No HTTP context available and no cached token");

        var authResult = await httpContext.AuthenticateAsync();
        if (!authResult.Succeeded || authResult.Properties is null)
            return new FailedResult("Could not authenticate to retrieve user tokens");

        var tokenResult = _storeTokensInProps.GetUserToken(authResult.Properties, parameters);

        if (tokenResult.Succeeded && tokenResult.Token.TokenForSpecifiedParameters is { } freshToken)
            _cache.Set(key, freshToken, freshToken.Expiration);

        return tokenResult;
    }

    public Task StoreTokenAsync(
        ClaimsPrincipal user,
        UserToken token,
        UserTokenRequestParameters? parameters = null,
        CancellationToken ct = default)
    {
        var key = CacheKey(user, parameters);
        if (key is not null)
            _cache.Set(key, token, token.Expiration);

        return Task.CompletedTask;
    }

    public Task ClearTokenAsync(
        ClaimsPrincipal user,
        UserTokenRequestParameters? parameters = null,
        CancellationToken ct = default)
    {
        var key = CacheKey(user, parameters);
        if (key is not null)
            _cache.Remove(key);

        return Task.CompletedTask;
    }

    private static string? CacheKey(ClaimsPrincipal user, UserTokenRequestParameters? parameters)
    {
        var sub = user.FindFirst("sub")?.Value
               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (sub is null) return null;

        var resource = parameters?.Resource?.ToString() ?? string.Empty;
        return $"user_token:{sub}:{resource}";
    }
}
