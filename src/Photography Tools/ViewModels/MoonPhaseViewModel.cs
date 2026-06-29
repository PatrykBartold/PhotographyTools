using CommunityToolkit.Maui;
using Photography_Tools.Services.KeyValueStoreService;
using System.Collections.Frozen;
using System.Text.Json;

namespace Photography_Tools.ViewModels;

public partial class MoonPhaseViewModel : AstroLocationViewModel
{
    private static readonly FrozenDictionary<string, string> MoonImagesNorth, MoonImagesSouth;
    private TimeSpan lastSelectedTime = TimeSpan.Zero;

    [ObservableProperty]
    private TimeSpan selectedTime = new(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, 0);

    [ObservableProperty]
    private string
        moonPhaseName = string.Empty,
        moonImage = string.Empty,
        illuminationPerc = string.Empty,
        moonAge = string.Empty,
        moonriseDate = string.Empty,
        moonsetDate = string.Empty;

    static MoonPhaseViewModel()
    {
        MoonImagesNorth = new Dictionary<string, string>
        {
            {"New Moon", "🌑"},
            {"Waxing Crescent", "🌒"},
            {"First Quarter", "🌓"},
            {"Waxing Gibbous", "🌔"},
            {"Full Moon", "🌕"},
            {"Waning Gibbous", "🌖"},
            {"Third Quarter", "🌗"},
            {"Waning Crescent", "🌘"},
        }.ToFrozenDictionary();

        MoonImagesSouth = new Dictionary<string, string>
        {
            {"New Moon", "🌑"},
            {"Waxing Crescent", "🌘"},
            {"First Quarter", "🌗"},
            {"Waxing Gibbous", "🌖"},
            {"Full Moon", "🌕"},
            {"Waning Gibbous", "🌔"},
            {"Third Quarter", "🌓"},
            {"Waning Crescent", "🌒"},
        }.ToFrozenDictionary();
    }

    public MoonPhaseViewModel([FromKeyedServices(KeyedServiceNames.OnlineAstroData)] IAstroDataService onlineAstroDataService, [FromKeyedServices(KeyedServiceNames.OfflineAstroData)] IAstroDataService offlineAstroDataService,
        IKeyValueStore<Place> locationsKeyValueStore, IPreferencesService preferencesService, IUiMessageService messageService, IPopupService popupService) : base(onlineAstroDataService, offlineAstroDataService, locationsKeyValueStore,
            preferencesService, messageService, popupService)
    {
        SelectedDate = DateTime.Today;
        LocationName = preferencesService.GetPreference(PreferencesKeys.MoonPhaseUserInputPreferencesKey, string.Empty) ?? string.Empty;
    }

    [RelayCommand]
    protected async Task OnAppearing()
    {
        if (IsPopupPresented)
            return;

        if (!CalculateCommand.IsRunning && CalculateCommand.CanExecute(null))
            await CalculateCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task OnSelectedTimeChangedAsync()
    {
        if (lastSelectedTime == SelectedTime)
            return;

        lastSelectedTime = SelectedTime;
        if (!CalculateCommand.IsRunning && CalculateCommand.CanExecute(null))
            await CalculateCommand.ExecuteAsync(null);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    protected override async Task CalculateAsync()
    {
        Place? location = await locationsKeyValueStore.GetValueAsync(LocationName);

        if (location is null)
        {
            return;
        }

        GeographicalCoordinates coordinates = location.Coordinates;
        DateTime date = SelectedDate.Date.Add(SelectedTime);

        ServiceResponse<MoonData?> offlineResult = await offlineAstroDataService.GetMoonDataAsync(date, coordinates.Latitude, coordinates.Longitude);

        if (offlineResult.IsSuccess && offlineResult.Data is not null)
            DisplayResult(offlineResult.Data, date, coordinates.Latitude, offlineAstroDataService.DataSourceInfo);

        if (!UseOnlineData)
            return;

        try
        {
            ServiceResponse<MoonData?> onlineResult = await onlineAstroDataService.GetMoonDataAsync(date, coordinates.Latitude, coordinates.Longitude);

            if (onlineResult.IsSuccess && onlineResult.Data is not null)
            {
                DisplayResult(onlineResult.Data, date, coordinates.Latitude, onlineAstroDataService.DataSourceInfo);
                return;
            }
            else
            {
                if (onlineResult.Code == -2)
                    return;

                await messageService.ShowMessageAsync("Online data source is not avaliable", $"{onlineResult.Message}\nLower accuracy data is only avaliable" ?? "Unexpected error occured\nOnly lower accuracy data is available", "Ok");
            }
        }
        catch (Exception ex)
        {
            if (ex is IndexOutOfRangeException or ArgumentNullException or JsonException or HttpRequestException or IOException)
                await messageService.ShowMessageAsync("Online data source is not avaliable", $"{ex.Message}\nOnly lower accuracy data is available", "Ok");
            else
                throw;
        }
    }

    public void DisplayResult(MoonData data, DateTime selectedDate, double latitude, string sourceInfo)
    {
        MoonPhaseName = data.Phase;
        SetMoonImage(data.Phase, latitude);
        SetIlluminationPerc(Math.Round(data.Illumination));
        SetMoonAge(Math.Round(data.MoonAge, 2));
        SetDataSourceInfo(sourceInfo);

        MoonriseDate = data.Rise.ToStringLocalTime(selectedDate.Day);
        MoonsetDate = data.Set.ToStringLocalTime(selectedDate.Day);
    }

    public void SetMoonImage(string phaseName, double latitude) =>
        MoonImage = latitude >= 0 ? MoonImagesNorth[phaseName] : MoonImagesSouth[phaseName];

    public void SetIlluminationPerc<T>(T value) =>
        IlluminationPerc = $"{value}%";

    public void SetMoonAge(double value) =>
        MoonAge = value < 2 ? $"{value} day" : $"{value} days";

    protected override void SaveUserInput()
    {
        if (!string.IsNullOrWhiteSpace(LocationName) && LocationName.Length <= 127)
            preferencesService.SetPreference(PreferencesKeys.MoonPhaseUserInputPreferencesKey, LocationName);
    }
}