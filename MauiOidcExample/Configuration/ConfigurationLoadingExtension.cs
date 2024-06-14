using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace MauiOidcExample.Configuration;

public static class ConfigurationLoadingExtension
{
    public static MauiAppBuilder AddMauiConfigurationFromJson(this MauiAppBuilder builder)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var appName = (string)(currentAssembly.CustomAttributes
            .SingleOrDefault(ca =>
                ca.ConstructorArguments.Any(a => a.Value is "Microsoft.Maui.ApplicationModel.AppInfo.Name"))
            ?.ConstructorArguments.LastOrDefault().Value ?? throw new Exception("Could not find app name."));

        var platform = DeviceInfo.Current.Platform.ToString();
        var names = new[]
        {
            $"{appName}.appsettings.json", $"{appName}.appsettings.{platform}.json",
            $"{appName}.appsettings.Development.json",
            $"{appName}.appsettings.{platform}.Development.json"
        }; // Keep the order in mind. Exclude Development appsettings from csproj based on configuration.

        var settings = names.Where(n =>
            currentAssembly.GetManifestResourceNames()
                .Any(rn => rn.Equals(n, StringComparison.InvariantCultureIgnoreCase)));

        foreach (var name in settings)
        {
            using var stream = currentAssembly.GetManifestResourceStream(name);
            if (stream == null) continue;

            var config = new ConfigurationBuilder();
            config.AddJsonStream(stream);
            builder.Configuration.AddConfiguration(config.Build());
        }

        return builder;
    }
}