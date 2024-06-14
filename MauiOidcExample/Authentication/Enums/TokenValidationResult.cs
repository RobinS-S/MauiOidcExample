namespace MauiOidcExample.Authentication.Enums;

public enum TokenResult
{
    Valid,
    Expired,
    NoInternet,
    OtherError
}

public enum LogoutResultStatus
{
    NoInternet,
    Success,
    UnknownError
}

public enum UserInfoResultType
{
    Success,
    Unauthorized,
    ServerError,
    NoInternet,
    OtherError
}

public enum RefreshTokenResult
{
    Success,
    NoInternet,
    Failed
}