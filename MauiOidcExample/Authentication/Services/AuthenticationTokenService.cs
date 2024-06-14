using MauiOidcExample.Authentication.Services.Interfaces;

namespace MauiOidcExample.Authentication.Services;

public class AuthenticationTokenService : IAuthenticationTokenService
{
    private const string AccessTokenStorageKey = "AuthenticationAccessToken";
    private const string AccessTokenExpirationStorageKey = "AuthenticationAccessTokenExpiration";
    private const string IdentityTokenStorageKey = "AuthenticationIdentityToken";
    private const string RefreshTokenStorageKey = "AuthenticationRefreshToken";

    private string? _accessToken;
    private DateTimeOffset? _accessTokenExpiration;
    private string? _identityToken;
    private string? _refreshToken;

    public async Task<bool> LoadTokensFromStorageAsync()
    {
        _accessToken = await SecureStorage.GetAsync(AccessTokenStorageKey);
        _identityToken = await SecureStorage.GetAsync(IdentityTokenStorageKey);
        _refreshToken = await SecureStorage.GetAsync(RefreshTokenStorageKey);
        _accessTokenExpiration = DateTimeOffset.TryParse(await SecureStorage.GetAsync(AccessTokenExpirationStorageKey),
            out var expiration)
            ? expiration
            : null;

        if (string.IsNullOrEmpty(_accessToken))
        {
            ClearCachedTokens();
            return false;
        }

        return true;
    }

    public async Task SaveTokensToStorageAsync(string accessToken, string identityToken, string refreshToken,
        DateTimeOffset tokenExpiration)
    {
        _accessToken = accessToken;
        _identityToken = identityToken;
        _refreshToken = refreshToken;
        _accessTokenExpiration = tokenExpiration;

        await SaveTokenAsync(AccessTokenStorageKey, accessToken);
        await SaveTokenAsync(IdentityTokenStorageKey, identityToken);
        await SaveTokenAsync(RefreshTokenStorageKey, refreshToken);
        await SaveTokenAsync(AccessTokenExpirationStorageKey, tokenExpiration.ToString());
    }

    public void ClearTokensFromStorage()
    {
        ClearCachedTokens();
        SecureStorage.Remove(AccessTokenStorageKey);
        SecureStorage.Remove(IdentityTokenStorageKey);
        SecureStorage.Remove(RefreshTokenStorageKey);
        SecureStorage.Remove(AccessTokenExpirationStorageKey);
    }

    public string? GetAccessToken()
    {
        return _accessToken;
    }

    public string? GetIdentityToken()
    {
        return _identityToken;
    }

    public string? GetRefreshToken()
    {
        return _refreshToken;
    }

    public DateTimeOffset? GetAccessTokenExpiration()
    {
        return _accessTokenExpiration;
    }

    private async Task SaveTokenAsync(string key, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            SecureStorage.Remove(key);
        else
            await SecureStorage.SetAsync(key, token);
    }

    private void ClearCachedTokens()
    {
        _accessToken = null;
        _identityToken = null;
        _refreshToken = null;
        _accessTokenExpiration = null;
    }
}