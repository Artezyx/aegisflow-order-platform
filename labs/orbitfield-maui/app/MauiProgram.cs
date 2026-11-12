using Microsoft.Extensions.Logging;
using OrbitField.App.Services;
using OrbitField.App.ViewModels;
namespace OrbitField.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder=MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddSingleton<LocalStore>();
        builder.Services.AddSingleton(new HttpClient{Timeout=TimeSpan.FromSeconds(12)});
        builder.Services.AddSingleton<SyncService>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        return builder.Build();
    }
}
