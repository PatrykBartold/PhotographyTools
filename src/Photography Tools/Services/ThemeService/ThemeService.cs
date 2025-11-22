#if ANDROID
using Microsoft.Maui.Platform;
#endif

namespace Photography_Tools.Services.ThemeService;

public sealed partial class ThemeService : IDisposable, IThemeService
{
    private readonly IPreferencesService preferencesService;

#if ANDROID
    private int? lastColor = -1, lastIsLight = -1;
#endif

    public ThemeService(IPreferencesService preferencesService)
    {
        this.preferencesService = preferencesService;

        if (Application.Current is null)
            return;

        Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
    }

    public void Dispose()
    {
        if (Application.Current is null)
            return;

        Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
    }

    private void OnRequestedThemeChanged(object? source, AppThemeChangedEventArgs args)
    {
        if (Application.Current is null)
            return;

        SetStatusBarColor();
    }

    public async Task SetThemeAsync()
    {
        int theme = Preferences.Get(PreferencesKeys.ThemeKey, 0);
        await SetThemeAsync(theme);
    }

    public async Task SetThemeAsync(int theme)
    {
        if (Application.Current is null)
            return;

        Application.Current.UserAppTheme = theme switch
        {
            IThemeService.Light => AppTheme.Light,
            IThemeService.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };

        if (preferencesService.GetPreference(PreferencesKeys.ThemeKey, 0) != theme)
            preferencesService.SetPreference(PreferencesKeys.ThemeKey, theme);

        await Task.Delay(50);
        SetStatusBarColor();
    }

    public void SetStatusBarColor()
    {
        if (Application.Current is null)
            return;

        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            SetStatusBarColor(Color.FromArgb("#000000"), false);
        }
        else
        {
            SetStatusBarColor(Color.FromArgb("#ffffff"), true);
        }
    }

    public void SetStatusBarColor(Color color, bool isLight)
    {
        if (MainThread.IsMainThread)
            SetStatusBarColorCore(color, isLight);
        else
            MainThread.BeginInvokeOnMainThread(() => SetStatusBarColorCore(color, isLight));
    }

    private void SetStatusBarColorCore(Color color, bool isLight)
    {
#if ANDROID
        if (!OperatingSystem.IsAndroidVersionAtLeast(21) || Platform.CurrentActivity is null)
            return;

        if (lastColor is not null && lastIsLight is not null && color.ToInt() == lastColor && lastIsLight == (isLight ? 1 : 0))
            return;

        lastColor = color.ToInt();
        lastIsLight = isLight ? 1 : 0;

        Android.App.Activity activity = Platform.CurrentActivity;
        Android.Views.Window? window = activity?.Window;
        if (activity is null || window is null)
            return;

        Android.Views.ViewGroup viewGroup = (Android.Views.ViewGroup)window.DecorView;
        window.AddFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);
        window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);

        if (OperatingSystem.IsAndroidVersionAtLeast(35))
        {
            Android.Views.View? statusBarOverlay = viewGroup.FindViewWithTag("StatusBarOverlay");

            if (statusBarOverlay is null)
            {
                int statusBarHeight = activity.Resources?.GetIdentifier("status_bar_height", "dimen", "android") ?? 0;
                int statusBarPixelSize = statusBarHeight > 0 ? activity.Resources?.GetDimensionPixelSize(statusBarHeight) ?? 0 : 0;

                statusBarOverlay = new(activity)
                {
                    LayoutParameters = new Android.Widget.FrameLayout.LayoutParams(Android.Views.ViewGroup.LayoutParams.MatchParent, statusBarPixelSize + 3)
                    {
                        Gravity = Android.Views.GravityFlags.Top
                    }
                };

                viewGroup.AddView(statusBarOverlay);
                statusBarOverlay.SetZ(0);
            }

            statusBarOverlay.SetBackgroundColor(color.ToPlatform());
        }
        else
        {
            window.SetStatusBarColor(color.ToPlatform());
        }

        new AndroidX.Core.View.WindowInsetsControllerCompat(window, viewGroup).AppearanceLightStatusBars = isLight;
#endif
        return;
    }
}
