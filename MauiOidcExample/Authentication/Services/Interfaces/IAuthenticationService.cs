using MauiOidcExample.Authentication.Enums;
using MauiOidcExample.Authentication.Models;

namespace MauiOidcExample.Authentication.Services.Interfaces;

public interface IAuthenticationService
{
    Task<TokenResult> ValidateTokenAsync();

    Task<UserInfoResult<T>> GetUserInfoAsync<T>() where T : class;

    Task<LoginResult> TryLoginAsync();

    Task<LogoutResult> TryLogoutAsync();

    bool IsAuthenticated();
}