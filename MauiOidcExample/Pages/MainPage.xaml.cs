using System.Security.Claims;
using MauiOidcExample.Authentication.Enums;
using MauiOidcExample.Authentication.Services.Interfaces;
using MauiOidcExample.Pages.ViewModels;

namespace MauiOidcExample.Pages;

public partial class MainPage : ContentPage
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthenticationTokenService _tokenService;
    private readonly MainPageViewModel _viewModel;

    public MainPage(IAuthenticationTokenService tokenService, IAuthenticationService authenticationService,
        MainPageViewModel viewModel)
    {
        InitializeComponent();

        _tokenService = tokenService;
        _authenticationService = authenticationService;
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        await _tokenService.LoadTokensFromStorageAsync();
        var hasToken = _tokenService.GetAccessToken() != null;
        if (!hasToken) return;

        var tokenResult = await _authenticationService.ValidateTokenAsync();
        await HandleTokenResult(tokenResult);
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var loginResult = await _authenticationService.TryLoginAsync();
            await HandleTokenResult(loginResult.Result, loginResult.User);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error during login", ex.Message, "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            var logoutResult = await _authenticationService.TryLogoutAsync();
            if (logoutResult.Status == LogoutResultStatus.Success)
                await SetAuthenticationState(false);
            else
                await DisplayAlert("Error during logout", "An error occurred during the logout: " + logoutResult.Status,
                    "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error during logout", ex.Message, "OK");
        }
    }

    private async Task HandleTokenResult(TokenResult result, ClaimsPrincipal? claimsPrincipal = null)
    {
        switch (result)
        {
            case TokenResult.Valid:
                var picture = claimsPrincipal?.FindFirst("picture")?.Value;
                await SetAuthenticationState(true);
                _viewModel.SetImageSourceCommand.Execute(
                    picture != null && Uri.IsWellFormedUriString(picture, UriKind.RelativeOrAbsolute)
                        ? ImageSource.FromUri(new Uri(picture))
                        : ImageSource.FromFile("dotnet_bot.png"));
                return;

            case TokenResult.NoInternet:
                _viewModel.SetErrorMessageCommand.Execute("No internet connection");
                break;

            case TokenResult.OtherError:
                _viewModel.SetErrorMessageCommand.Execute("An error occurred during token validation");
                break;

            case TokenResult.Expired:
                _viewModel.SetErrorMessageCommand.Execute("Token has expired");
                break;

            default:
                _viewModel.SetErrorMessageCommand.Execute("Token is invalid");
                break;
        }

        await SetAuthenticationState(false);
    }

    private async Task SetAuthenticationState(bool state)
    {
        _viewModel.SetAuthenticatedCommand.Execute(state);

        if (state)
        {
            _viewModel.SetErrorMessageCommand.Execute(null);
            await _viewModel.RetrieveDataCommand.ExecuteAsync(null);
        }
        else
        {
            _viewModel.SetImageSourceCommand.Execute(ImageSource.FromFile("dotnet_bot.png"));
        }
    }
}