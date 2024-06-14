namespace MauiOidcExample.Configuration;

public record MauiAuthConfiguration
{
    public string? ApiUrl { get; init; }
    public string AppCallbackUrl { get; init; } = null!;
    public MauiOidcConfiguration Oidc { get; init; } = null!;
}