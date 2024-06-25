using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiOidcExample.Repositories;

namespace MauiOidcExample.Pages.ViewModels;

public partial class MainPageViewModel(WeatherForecastRepository weatherForecastRepository) : ObservableObject
{
    [ObservableProperty] private DateTimeOffset? _date;

    [ObservableProperty] private string? _errorMessage;

    [ObservableProperty] private ImageSource _imageSource = ImageSource.FromFile("dotnet_bot.png");

    [ObservableProperty] private bool _isAuthenticated;

    [ObservableProperty] private string? _summary;

    [ObservableProperty] private int? _temperatureC;

    [ObservableProperty] private int? _temperatureF;

    [RelayCommand]
    private async Task RetrieveData()
    {
        try
        {
            var weatherForecast = await weatherForecastRepository.GetWeatherForecast();

            ErrorMessage = null;
            Summary = weatherForecast?.Summary;
            Date = weatherForecast?.Date;
            TemperatureC = weatherForecast?.TemperatureC;
            TemperatureF = weatherForecast?.TemperatureF;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void SetAuthenticated(bool isAuthenticated)
    {
        IsAuthenticated = isAuthenticated;
    }

    [RelayCommand]
    private void SetImageSource(ImageSource imageSource)
    {
        ImageSource = imageSource;
    }

    [RelayCommand]
    private void SetErrorMessage(string? errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}