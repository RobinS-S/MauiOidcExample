using System.Text.Json;
using MauiOidcExample.Configuration;
using MauiOidcExample.Shared;

namespace MauiOidcExample.Repositories
{
    public class WeatherForecastRepository(
        MauiAuthConfiguration authConfiguration,
        IHttpClientFactory httpClientFactory,
        JsonSerializerOptions serializerOptions)
    {
        public async Task<WeatherForecast?> GetWeatherForecast()
        {

            var client = httpClientFactory.CreateClient(Constants.AuthenticatedHttpClientName);
            if (authConfiguration.ApiUrl != null) client.BaseAddress = new Uri(authConfiguration.ApiUrl);

            try
            {
                var response = await client.GetAsync("WeatherForecast");
                if (!response.IsSuccessStatusCode)
                    throw new Exception(
                        $"An error occurred during the request: {(int)response.StatusCode} {response.StatusCode}");

                var text = await response.Content.ReadAsStringAsync();
                var weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(text, serializerOptions);

                return weatherForecast;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred during the request: {ex.Message}");
            }
        }
    }
}
