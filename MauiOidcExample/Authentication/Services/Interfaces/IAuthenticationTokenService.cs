namespace MauiOidcExample.Authentication.Services.Interfaces;

public interface IAuthenticationTokenService
{
    Task<bool> LoadTokensFromStorageAsync();

    Task SaveTokensToStorageAsync(string accessToken, string identityToken, string refreshToken,
        DateTimeOffset tokenExpiration);

    void ClearTokensFromStorage();

    string? GetAccessToken();

    string? GetIdentityToken();

    string? GetRefreshToken();

    DateTimeOffset? GetAccessTokenExpiration();
}