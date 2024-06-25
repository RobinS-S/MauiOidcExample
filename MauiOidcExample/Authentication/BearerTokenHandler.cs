using System.Net.Http.Headers;
using MauiOidcExample.Authentication.Services.Interfaces;

namespace MauiOidcExample.Authentication;

public class BearerTokenHandler(IAuthenticationTokenService tokenService) : DelegatingHandler
{
    private readonly IAuthenticationTokenService _tokenService = tokenService;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _tokenService.GetAccessToken();
        if (token != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}