using CommunityToolkit.Maui;
using Photography_Tools.Services.KeyValueStoreService;
using System.Text.Json;

namespace Photography_Tools.ViewModels;

public partial class SunViewModel : AstroLocationViewModel
{
    [ObservableProperty]
    private string
        sunriseDate = string.Empty,
        sunsetDate = string.Empty,
        morningGoldenHourStartDate = string.Empty,
        morningGoldenHourEndDate = string.Empty,
        eveningGoldenHourStartDate = string.Empty,
        eveningGoldenHourEndDate = string.Empty,
        morningBlueHourStartDate = string.Empty,
        morningBlueHourEndDate = string.Empty,
        eveningBlueHourStartDate = string.Empty,
        eveningBlueHourEndDate = string.Empty,
        morningCivilTwilightStartDate = string.Empty,
        morningCivilTwilightEndDate = string.Empty,
        eveningCivilTwilightStartDate = string.Empty,
        eveningCivilTwilightEndDate = string.Empty,
        upperTransitDate = string.Empty;

    public SunViewModel([FromKeyedServices(KeyedServiceNames.OnlineAstroData)] IAstroDataService onlineAstroDataService, [FromKeyedServices(KeyedServiceNames.OfflineAstroData)] IAstroDataService offlineAstroDataService,
        IKeyValueStore<Place> locationsKeyValueStore, IPreferencesService preferencesService, IUiMessageService messageService, IPopupService popupService) : base(onlineAstroDataService, offlineAstroDataService, locationsKeyValueStore,
            preferencesService, messageService, popupService)
    {
        SelectedDate = DateTime.Today.AddHours(12);
        LocationName = preferencesService.GetPreference(PreferencesKeys.SunUserInputPreferencesKey, string.Empty) ?? string.Empty;
    }

    [RelayCommand]
    protected async Task OnAppearingAsync()
    {
        if (IsPopupPresented)
            return;

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
        DateTime date = SelectedDate.Date.AddHours(12);

        ServiceResponse<SunPhasesResult?> offlineResult = await offlineAstroDataService.GetSunDataAsync(date, coordinates.Latitude, coordinates.Longitude);

        if (offlineResult.IsSuccess && offlineResult.Data is not null)
            DisplayResult(offlineResult.Data, date, offlineAstroDataService.DataSourceInfo);

        if (!UseOnlineData)
            return;

        try
        {
            ServiceResponse<SunPhasesResult?> onlineResult = await onlineAstroDataService.GetSunDataAsync(date, coordinates.Latitude, coordinates.Longitude);

            if (onlineResult.IsSuccess && onlineResult.Data is not null)
            {
                DisplayResult(onlineResult.Data, date, onlineAstroDataService.DataSourceInfo);
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

    public void DisplayResult(SunPhasesResult data, DateTime selectedDate, string sourceInfo)
    {
        SetDataSourceInfo(sourceInfo);

        SunriseDate = data.Day.StartDate.ToStringLocalTime(selectedDate.Day);
        UpperTransitDate = data.UpperTransit.ToStringLocalTime(selectedDate.Day);
        SunsetDate = data.Day.EndDate.ToStringLocalTime(selectedDate.Day);

        MorningCivilTwilightStartDate = data.MorningCivilTwilight.StartDate.ToStringLocalTime(selectedDate.Day);
        MorningCivilTwilightEndDate = data.MorningCivilTwilight.EndDate.ToStringLocalTime(selectedDate.Day);

        MorningBlueHourStartDate = data.MorningBlueHour.StartDate.ToStringLocalTime(selectedDate.Day);
        MorningBlueHourEndDate = data.MorningBlueHour.EndDate.ToStringLocalTime(selectedDate.Day);

        MorningGoldenHourStartDate = data.MorningGoldenHour.StartDate.ToStringLocalTime(selectedDate.Day);
        MorningGoldenHourEndDate = data.MorningGoldenHour.EndDate.ToStringLocalTime(selectedDate.Day);

        EveningGoldenHourStartDate = data.EveningGoldenHour.StartDate.ToStringLocalTime(selectedDate.Day);
        EveningGoldenHourEndDate = data.EveningGoldenHour.EndDate.ToStringLocalTime(selectedDate.Day);

        EveningBlueHourStartDate = data.EveningBlueHour.StartDate.ToStringLocalTime(selectedDate.Day);
        EveningBlueHourEndDate = data.EveningBlueHour.EndDate.ToStringLocalTime(selectedDate.Day);

        EveningCivilTwilightStartDate = data.EveningCivilTwilight.StartDate.ToStringLocalTime(selectedDate.Day);
        EveningCivilTwilightEndDate = data.EveningCivilTwilight.EndDate.ToStringLocalTime(selectedDate.Day);
    }

    protected override void SaveUserInput()
    {
        if (!string.IsNullOrWhiteSpace(LocationName) && LocationName.Length <= 127)
            preferencesService.SetPreference(PreferencesKeys.SunUserInputPreferencesKey, LocationName);
    }
}