using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Photography_Tools.Components.Popups;
using Photography_Tools.DataAccess.AstroDataAccess;
using Photography_Tools.Services.ConfigService;
using Photography_Tools.Services.KeyValueStoreService;
using Photography_Tools.Services.ThemeService;

namespace Photography_Tools;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit(static options =>
            {
                options.SetPopupOptionsDefaults(new DefaultPopupOptionsSettings { Shape = null });
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("NotoColorEmojiSubsetted.ttf", "NotoColorEmoji");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // DataAccess
        builder.Services.AddSingleton<IAstroDataAccess, UsnoAstroDataAccess>();
        builder.Services.AddSingleton<INDFiltersDataAccess, StaticNDFiltersDataAccess>();
        builder.Services.AddSingleton<ISensorsDataAccess, StaticSensorsDataAccess>();
        builder.Services.AddSingleton(Preferences.Default);

        // Pages
        builder.Services.AddSingleton<AstroTimeCalcPage>();
        builder.Services.AddSingleton<DofCalcPage>();
        builder.Services.AddSingleton<NdFilterCalcPage>();
        builder.Services.AddSingleton<MoonPhasePage>();
        builder.Services.AddSingleton<SunPage>();
        builder.Services.AddSingleton<TimeLapseCalcPage>();
        builder.Services.AddSingleton<SettingsPage>();

        // Services
        builder.Services.AddKeyedSingleton<IAstroDataService, OfflineAstroDataService>(KeyedServiceNames.OfflineAstroData);
        builder.Services.AddKeyedSingleton<IAstroDataService, OnlineAstroDataService>(KeyedServiceNames.OnlineAstroData);
        builder.Services.AddSingleton(KeyValueStoreFactory.CreateCacheAstroDataKeyValueStore());
        builder.Services.AddSingleton(KeyValueStoreFactory.CreateLocationKeyValueStore());
        builder.Services.AddSingleton<IPhotographyCalculationsService, PhotographyCalculationsService>();
        builder.Services.AddSingleton<IPreferencesService, PreferencesService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IUiMessageService, UiMessageService>();

        // ViewModel
        builder.Services.AddSingleton<AstroTimeCalcViewModel>();
        builder.Services.AddSingleton<DofCalcViewModel>();
        builder.Services.AddSingleton<NDFilterCalcViewModel>();
        builder.Services.AddSingleton<MoonPhaseViewModel>();
        builder.Services.AddSingleton<SunViewModel>();
        builder.Services.AddSingleton<TimeLapseCalculatorViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();

        // Popups
        builder.Services.AddSingletonPopup<LocationPopup>();

        // HttpClient
        builder.Services.AddHttpClient(HttpClientConst.UsnoHttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://aa.usno.navy.mil/api/");
        });

        return builder.Build();
    }
}
