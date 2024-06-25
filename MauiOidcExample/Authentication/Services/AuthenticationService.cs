using System.Net;
using System.Text.Json;
using System.Timers;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using MauiOidcExample.Authentication.Enums;
using MauiOidcExample.Authentication.Models;
using MauiOidcExample.Authentication.Services.Interfaces;
using MauiOidcExample.Configuration;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;
using LoginResult = MauiOidcExample.Authentication.Models.LoginResult;
using LogoutResult = MauiOidcExample.Authentication.Models.LogoutResult;

namespace MauiOidcExample.Authentication.Services;

public class AuthenticationService : IAuthenticationService, IDisposable
{
    private readonly OidcClient _authClient;
    private readonly MauiAuthConfiguration _authConfiguration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly Timer _tokenRefreshTimer;
    private readonly IAuthenticationTokenService _tokenService;
    private ElapsedEventHandler? _timerHandler;

    public AuthenticationService(
        MauiAuthConfiguration authConfiguration,
        IAuthenticationTokenService tokenService,
        OidcClient authClient,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthenticationService> logger)
    {
        _authConfiguration = authConfiguration;
        _tokenService = tokenService;
        _authClient = authClient;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _tokenRefreshTimer = new Timer { AutoReset = false };
        ScheduleNextRefresh();
    }

    public async Task<TokenResult> ValidateTokenAsync()
    {
        if (!EnsureNetworkAvailable()) return TokenResult.NoInternet;

        var token = _tokenService.GetAccessToken();
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Access token is null or empty.");
            return TokenResult.OtherError;
        }

        var expiration = _tokenService.GetAccessTokenExpiration();
        var refreshToken = _tokenService.GetRefreshToken();
        if (expiration == null || expiration < DateTimeOffset.UtcNow)
        {
            _logger.LogInformation("Access token has expired or no expiration found.");
            return await RefreshOrLogoutAsync(refreshToken);
        }

        var userInfoResult = await GetUserInfoAsync<object>();
        if (userInfoResult.ResultType == UserInfoResultType.Unauthorized)
        {
            _logger.LogInformation("User info request unauthorized, attempting token refresh.");
            return await RefreshOrLogoutAsync(refreshToken);
        }

        ScheduleNextRefresh();
        return TokenResult.Valid;
    }

    public async Task<UserInfoResult<T>> GetUserInfoAsync<T>() where T : class
    {
        if (!EnsureNetworkAvailable()) return new UserInfoResult<T>(UserInfoResultType.NoInternet, null);

        var request = new HttpRequestMessage(HttpMethod.Get, _authConfiguration.Oidc.UserInfoUrl);
        var httpClient = _httpClientFactory.CreateClient(Constants.AuthenticatedHttpClientName);

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var status = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => UserInfoResultType.Unauthorized,
                HttpStatusCode.InternalServerError => UserInfoResultType.ServerError,
                _ => UserInfoResultType.OtherError
            };
            _logger.LogWarning("User info request failed with status code: {StatusCode}", response.StatusCode);
            return new UserInfoResult<T>(status, null);
        }

        var content = await response.Content.ReadAsStringAsync();
        var value = JsonSerializer.Deserialize<T>(content);

        _logger.LogInformation("User info request successful.");
        return new UserInfoResult<T>(UserInfoResultType.Success, value);
    }

    public async Task<LoginResult> TryLoginAsync()
    {
        if (!EnsureNetworkAvailable()) return new LoginResult(TokenResult.NoInternet);

        var loginResult = await _authClient.LoginAsync(new LoginRequest
        {
            FrontChannelExtraParameters = new Parameters(_authConfiguration.Oidc.FrontChannelExtraParameters ?? []),
            BackChannelExtraParameters = new Parameters(_authConfiguration.Oidc.BackChannelExtraParameters ?? [])
        });
        ProcessRedirectionWindowsFocus();

        if (loginResult.IsError)
        {
            _logger.LogError("Login error: {Error}", loginResult.Error);
            return new LoginResult(TokenResult.OtherError, loginResult.Error);
        }

        await _tokenService.SaveTokensToStorageAsync(loginResult.AccessToken, loginResult.IdentityToken,
            loginResult.RefreshToken, loginResult.AccessTokenExpiration);
        ScheduleNextRefresh();
        _logger.LogInformation("Login successful.");
        return new LoginResult(TokenResult.Valid, null, loginResult.AccessTokenExpiration, loginResult.User);
    }

    public async Task<LogoutResult> TryLogoutAsync()
    {
        if (!EnsureNetworkAvailable()) return new LogoutResult(LogoutResultStatus.NoInternet);

        var result = await _authClient.LogoutAsync(new LogoutRequest());
        ProcessRedirectionWindowsFocus();

        if (result.IsError)
        {
            _logger.LogError("Logout error: {Error}", result.Error);
            return new LogoutResult(LogoutResultStatus.UnknownError, result.Error);
        }

        SetLoggedOut();
        _logger.LogInformation("Logout successful.");
        return new LogoutResult(LogoutResultStatus.Success);
    }

    public bool IsAuthenticated()
    {
        var authToken = _tokenService.GetAccessToken();
        return !string.IsNullOrEmpty(authToken);
    }

    public void Dispose()
    {
        _tokenRefreshTimer.Dispose();
    }

    private void ScheduleNextRefresh()
    {
        if (_timerHandler != null) _tokenRefreshTimer.Elapsed -= _timerHandler;

        _tokenRefreshTimer.Stop();

        var expiration = _tokenService.GetAccessTokenExpiration();
        if (!expiration.HasValue || expiration.Value == DateTimeOffset.MinValue)
        {
            _logger.LogInformation("No valid access token expiration found, skipping refresh scheduling.");
            return;
        }

        var timeToExpiry = expiration.Value - DateTimeOffset.UtcNow;
        var refreshInterval = Math.Max(timeToExpiry.TotalMilliseconds / 2, timeToExpiry.TotalMilliseconds - 60000);

        _tokenRefreshTimer.Interval = refreshInterval > 0 ? refreshInterval : 1000;
        _logger.LogInformation("Scheduled next token refresh in {RefreshInterval} milliseconds.",
            _tokenRefreshTimer.Interval);

        _timerHandler = async (_, _) => await CheckAndRefreshToken();
        _tokenRefreshTimer.Elapsed += _timerHandler;
        _tokenRefreshTimer.AutoReset = false;
        _tokenRefreshTimer.Start();
    }

    private async Task CheckAndRefreshToken()
    {
        _logger.LogInformation("Checking and refreshing token if necessary.");
        var expiration = _tokenService.GetAccessTokenExpiration();
        if (expiration == null)
        {
            _logger.LogWarning("Access token expiration not found.");
            return;
        }

        var halfExpiration = expiration.Value -
                             TimeSpan.FromSeconds((expiration.Value - DateTimeOffset.UtcNow).TotalSeconds / 2);
        if (DateTimeOffset.UtcNow >= halfExpiration) await TryRefreshTokenAsync();
    }

    private bool EnsureNetworkAvailable()
    {
        var networkAccess = Connectivity.Current.NetworkAccess;

        if (networkAccess == NetworkAccess.Internet) 
            return true;

        _logger.LogWarning("No network access available.");
        return false;
    }

    private async Task<TokenResult> RefreshOrLogoutAsync(string? refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken)) return await TryRefreshTokenAsync();

        _logger.LogWarning("Refresh token is null or empty, logging out.");
        SetLoggedOut();
        return TokenResult.Expired;
    }

    private async Task<TokenResult> TryRefreshTokenAsync()
    {
        var refreshToken = _tokenService.GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Refresh token is null or empty, logging out.");
            SetLoggedOut();
            return TokenResult.Expired;
        }

        var refreshResult = await RefreshTokenAsync(refreshToken);
        if (refreshResult == RefreshTokenResult.Success)
        {
            ScheduleNextRefresh();
            return TokenResult.Valid;
        }

        _logger.LogWarning("Token refresh failed, logging out.");
        SetLoggedOut();
        return TokenResult.Expired;
    }

    private async Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken)
    {
        if (!EnsureNetworkAvailable()) return RefreshTokenResult.NoInternet;

        var result = await _authClient.RefreshTokenAsync(refreshToken,
            new Parameters(_authConfiguration.Oidc.BackChannelExtraParameters ?? []));

        if (result.IsError)
        {
            _logger.LogError("Token refresh error: {Error}", result.Error);
            return RefreshTokenResult.Failed;
        }

        await _tokenService.SaveTokensToStorageAsync(result.AccessToken, result.IdentityToken, result.RefreshToken,
            result.AccessTokenExpiration);
        _logger.LogInformation("Token refresh successful.");
        return RefreshTokenResult.Success;
    }

    private void SetLoggedOut()
    {
        _tokenService.ClearTokensFromStorage();
        _tokenRefreshTimer.Stop();
        _logger.LogInformation("User logged out and tokens cleared.");
    }

    private void ProcessRedirectionWindowsFocus()
    {
#if WINDOWS
        WinUI.App.ActivateApplication();
#endif
        _logger.LogInformation("Processed redirection windows focus.");
    }
}