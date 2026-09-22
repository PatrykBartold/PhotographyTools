namespace Photography_Tools.Services.UiMessageService;

public class UiMessageService : IUiMessageService
{
    public async Task ShowMessageAsync(string title, string message, string cancel = "Cancel")
    {
        Page? mainPage = UiHelper.GetMainPage();

        if (mainPage is null)
            return;

        if (MainThread.IsMainThread)
            await mainPage.DisplayAlertAsync(title, message, cancel);
        else
            MainThread.BeginInvokeOnMainThread(async () => { await mainPage.DisplayAlertAsync(title, message, cancel); });
    }

    public async Task<bool> ShowMessageAsync(string title, string message, string accept = "Ok", string cancel = "Cancel")
    {
        Page? mainPage = UiHelper.GetMainPage();

        if (mainPage is null)
            return false;

        if (MainThread.IsMainThread)
            return await mainPage.DisplayAlertAsync(title, message, accept, cancel);

        bool result = false;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            result = await mainPage.DisplayAlertAsync(title, message, accept, cancel);
        });

        return result;
    }

    public void ShowMessageAndForget(string title, string message, string cancel = "Cancel")
    {
        Page? mainPage = UiHelper.GetMainPage();

        if (mainPage is null)
            return;

        mainPage.Dispatcher.Dispatch(async () =>
            await ShowMessageAsync(title, message, cancel)
        );
    }
}
