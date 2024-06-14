using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiOidcExample.Configuration;
using MauiOidcExample.Shared;

namespace MauiOidcExample.Pages;

public partial class MainPageViewModel : ObservableObject
{
    private readonly MauiAuthConfiguration _authConfiguration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _serializerOptions;

    [ObservableProperty] private DateTimeOffset? _date;

    [ObservableProperty] private string? _errorMessage;

    [ObservableProperty] private string? _summary;

    [ObservableProperty] private int? _temperatureC;

    [ObservableProperty] private int? _temperatureF;

    public MainPageViewModel(MauiAuthConfiguration authConfiguration, IHttpClientFactory httpClientFactory,
        JsonSerializerOptions serializerOptions)
    {
        _authConfiguration = authConfiguration;
        _httpClientFactory = httpClientFactory;
        _serializerOptions = serializerOptions;
    }

    [RelayCommand]
    private async Task RetrieveData()
    {
        var client = _httpClientFactory.CreateClient(Constants.AuthenticatedHttpClientName);
        if (_authConfiguration.ApiUrl != null) client.BaseAddress = new Uri(_authConfiguration.ApiUrl);

        try
        {
            var response = await client.GetAsync("WeatherForecast"); // Without ApiUrl, you could call other APIs here
            if (response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                var weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(text, _serializerOptions);

                ErrorMessage = null;
                Summary = weatherForecast?.Summary;
                Date = weatherForecast?.Date;
                TemperatureC = weatherForecast?.TemperatureC;
                TemperatureF = weatherForecast?.TemperatureF;
            }
            else
            {
                ErrorMessage =
                    $"An error occurred during the request: {(int)response.StatusCode} {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred during the request: {ex.Message}";
        }
    }
}