using IdentityModel.Client;
using IdentityModel.OidcClient.Browser;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;

#if WINDOWS
using WebAuthenticator = WinUIEx.WebAuthenticator;
#endif

namespace MauiOidcExample.Authentication;

public class AuthWorkaroundBrowser : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        var authResult =
            await WebAuthenticator
#if !WINDOWS
                .Default
#endif
                .AuthenticateAsync(new Uri(options.StartUrl), new Uri(options.EndUrl)
#if WINDOWS
                    , cancellationToken
#endif
                );
        var url = new RequestUrl(options.EndUrl)
            .Create(new Parameters(authResult.Properties));

        return new BrowserResult
        {
            Response = url
        };
    }
}