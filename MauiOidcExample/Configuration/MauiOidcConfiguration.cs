namespace MauiOidcExample.Configuration;

public record MauiOidcConfiguration
{
    public string Authority { get; init; } = null!; // Your Auth0 domain
    public Dictionary<string, string>? BackChannelExtraParameters { get; init; }
    public string ClientId { get; init; } = null!; // Get this from Auth0 configuration page

    public Dictionary<string, string>? FrontChannelExtraParameters
    {
        get;
        init;
    } // For Auth0, add "audience" with the value being your app host without port

    public string[] Scopes { get; init; } = null!; // For Auth0, use "openid", "offline_access", "profile", "email"
    public string UserInfoUrl { get; init; } = null!; // For Auth0, use {Authority}/userinfo
}