using System.Text.Json;
using CommunityToolkit.Maui;
using IdentityModel.OidcClient;
using MauiOidcExample.Authentication;
using MauiOidcExample.Authentication.Services;
using MauiOidcExample.Authentication.Services.Interfaces;
using MauiOidcExample.Configuration;
using MauiOidcExample.Pages;
using MauiOidcExample.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MainPageViewModel = MauiOidcExample.Pages.ViewModels.MainPageViewModel;
#if WINDOWS
using Microsoft.Maui.LifecycleEvents;
#endif

namespace MauiOidcExample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .AddMauiConfigurationFromJson()

#if WINDOWS
            // Authentication Windows focus after redirection workaround
            .ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windows => windows.OnWindowCreated(window =>
                {
                    WinUI.App.CurrentWindow = window as MauiWinUIWindow;
                }));
            })
#endif
            ;
#if DEBUG
        builder.Logging.AddDebug();
#endif

        var authConfiguration = builder.Configuration.GetRequiredSection("MauiAuth")
            .Get<MauiAuthConfiguration>() ?? throw new Exception(
            "Missing MAUI auth configuration appsettings.json, not in root or its build action not set to embedded resource!");
        builder.Services.AddSingleton(authConfiguration);

        builder.Services.AddTransient<WeatherForecastRepository>();
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();

        builder.Services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Authentication
        builder.Services.AddSingleton(new OidcClient(new OidcClientOptions
        {
            Authority = authConfiguration.Oidc.Authority,
            ClientId = authConfiguration.Oidc.ClientId,
            RedirectUri = $"{Constants.AppProtocolName}://{authConfiguration.AppCallbackUrl}/",
            PostLogoutRedirectUri =
                $"{Constants.AppProtocolName}://{authConfiguration.AppCallbackUrl}/",
            Scope = string.Join(" ", authConfiguration.Oidc.Scopes),
            Browser = new AuthWorkaroundBrowser()
        }));

        // Authenticated HTTP requests
        builder.Services.AddSingleton<IAuthenticationTokenService, AuthenticationTokenService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddTransient<BearerTokenHandler>();
        builder.Services.AddHttpClient(Constants.AuthenticatedHttpClientName, client => { })
            .AddHttpMessageHandler<BearerTokenHandler>();

        return builder.Build();
    }
}