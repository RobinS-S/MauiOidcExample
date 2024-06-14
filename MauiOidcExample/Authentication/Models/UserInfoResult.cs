using System.Security.Claims;
using MauiOidcExample.Authentication.Enums;

namespace MauiOidcExample.Authentication.Models;

public class UserInfoResult<T> where T : class
{
    public UserInfoResult(UserInfoResultType resultType, T? result)
    {
        ResultType = resultType;
        Result = result;
    }

    public T? Result { get; set; }

    public UserInfoResultType ResultType { get; set; }
}

public class LoginResult
{
    public LoginResult(TokenResult result, string? errorMessage = null, DateTimeOffset? tokenExpiration = null,
        ClaimsPrincipal? user = null)
    {
        Result = result;
        ErrorMessage = errorMessage;
        TokenExpiration = tokenExpiration;
        User = user;
    }

    public TokenResult Result { get; set; }

    public DateTimeOffset? TokenExpiration { get; set; }

    public string? ErrorMessage { get; set; }

    public ClaimsPrincipal? User { get; set; }
}

public class LogoutResult
{
    public LogoutResult(LogoutResultStatus status, string? errorMessage = null)
    {
        Status = status;
        ErrorMessage = errorMessage;
    }

    public LogoutResultStatus Status { get; set; }

    public string? ErrorMessage { get; set; }
}