using Android.App;
using Android.Content.PM;
using Content = Android.Content;

namespace MauiOidcExample.Platforms.Android;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Content.Intent.ActionView },
    Categories = new[]
    {
        Content.Intent.CategoryDefault,
        Content.Intent.CategoryBrowsable
    },
    DataScheme = Constants.AppProtocolName)]
public class WebAuthenticationCallbackActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
{
}