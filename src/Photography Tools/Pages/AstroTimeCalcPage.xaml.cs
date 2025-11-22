using Photography_Tools.Services.ThemeService;

namespace Photography_Tools.Pages;

public partial class AstroTimeCalcPage : ContentPage
{
    private readonly IThemeService themeService;

    public AstroTimeCalcPage(AstroTimeCalcViewModel astroTimeCalcViewModel, IThemeService themeService)
    {
        InitializeComponent();
        BindingContext = astroTimeCalcViewModel;
        this.themeService = themeService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await themeService.SetThemeAsync();
    }
}