using CommunityToolkit.Maui;
using Fooddrink.Pages;
using Fooddrink.Services;
using Microsoft.Extensions.Logging;

namespace Fooddrink;

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
            });

        // Register services as singletons
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<HardwareService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<JsonStorageService>();

        // Register pages for DI
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<CameraPage>();
        builder.Services.AddTransient<LocationPage>();
        builder.Services.AddTransient<ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
