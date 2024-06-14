using MauiOidcExample.Authentication.Enums;
using MauiOidcExample.Authentication.Services.Interfaces;

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
        if (hasToken)
        {
            var tokenResult = await _authenticationService.ValidateTokenAsync();
            switch (tokenResult)
            {
                case TokenResult.Valid:
                    LoginView.IsVisible = false;
                    HomeView.IsVisible = true;
                    WelcomeLabel.IsVisible = true;
                    await _viewModel.RetrieveDataCommand.ExecuteAsync(null);
                    break;
                case TokenResult.NoInternet:
                    _viewModel.ErrorMessage = "No internet connection";
                    break;
                case TokenResult.OtherError:
                    _viewModel.ErrorMessage = "An error occurred during token validation";
                    break;
                case TokenResult.Expired:
                    _viewModel.ErrorMessage = "Token has expired";
                    break;
                default:
                    _viewModel.ErrorMessage = "Token is invalid";
                    break;
            }
        }
        else
        {
            LoginView.IsVisible = true;
            HomeView.IsVisible = false;
            WelcomeLabel.IsVisible = false;
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var loginResult = await _authenticationService.TryLoginAsync();

            if (loginResult.Result == TokenResult.Valid)
            {
                LoginView.IsVisible = false;
                HomeView.IsVisible = true;
                WelcomeLabel.IsVisible = true;

                var picture = loginResult.User?.FindFirst("picture")?.Value;
                if (picture != null && Uri.IsWellFormedUriString(picture, UriKind.RelativeOrAbsolute))
                    Picture.Source = ImageSource.FromUri(new Uri(picture));
                else
                    Picture.Source = "dotnet_bot.png";

                await _viewModel.RetrieveDataCommand.ExecuteAsync(null);
            }
            else
            {
                Picture.Source = "dotnet_bot.png";
                await DisplayAlert("Error during login", loginResult.ErrorMessage, "OK");
            }
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
            {
                HomeView.IsVisible = false;
                LoginView.IsVisible = true;
                WelcomeLabel.IsVisible = true;
            }
            else
            {
                await DisplayAlert("Error during logout", "An error occurred during the logout: " + logoutResult.Status,
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error during logout", ex.Message, "OK");
        }
    }
}