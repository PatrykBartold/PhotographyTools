using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Photography_Tools.Platforms.Android;

namespace Photography_Tools;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        OnBackPressedDispatcher.AddCallback(this, new BackPress(this));
    }

    public override async void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);

        Window?.DecorView?.SetBackgroundColor((newConfig.UiMode & UiMode.NightMask) == UiMode.NightYes ?
                Android.Graphics.Color.Black : Android.Graphics.Color.White);
    }
}
