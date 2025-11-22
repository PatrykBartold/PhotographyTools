namespace Photography_Tools.Services.ThemeService;

public interface IThemeService
{
    public const int System = 0, Light = 1, Dark = 2;

    void SetStatusBarColor();
    void SetStatusBarColor(Color color, bool isLight);
    Task SetThemeAsync();
    Task SetThemeAsync(int theme);
}