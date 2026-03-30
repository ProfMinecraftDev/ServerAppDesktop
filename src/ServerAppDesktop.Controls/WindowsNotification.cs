namespace ServerAppDesktop.Controls;

public sealed class WindowsNotification
{
    private AppNotification? notification;

    public AppNotificationScenario NotificationScenario
    {
        get;
        set { field = value; RebuildNotification(); }
    } = AppNotificationScenario.Default;

    public Uri? AppLogoUri
    {
        get;
        set { field = value; RebuildNotification(); }
    }

    public Uri? HeroImagerUri
    {
        get;
        set { field = value; RebuildNotification(); }
    }

    public string Title
    {
        get;
        set { field = value; RebuildNotification(); }
    } = ResourceHelper.GetString("Notify_Default_Title");

    public string Message
    {
        get;
        set { field = value; RebuildNotification(); }
    } = ResourceHelper.GetString("Notify_Default_Msg");

    public string AttributionText
    {
        get;
        set { field = value; RebuildNotification(); }
    } = "";

    public AppNotificationSoundEvent SoundEvent
    {
        get;
        set { field = value; RebuildNotification(); }
    } = AppNotificationSoundEvent.Default;

    public AppNotificationDuration Duration
    {
        get;
        set { field = value; RebuildNotification(); }
    } = AppNotificationDuration.Default;

    public DateTime TimeStamp
    {
        get;
        set { field = value; RebuildNotification(); }
    } = DateTime.Now;

    public AppNotification Notification => notification ?? throw new InvalidOperationException(ResourceHelper.GetString("Err_NotificationNotBuilt"));
    public AppNotificationBuilder NotificationToBuild { get => field ?? throw new InvalidOperationException(ResourceHelper.GetString("Err_NotificationNotBuilt")); private set; }

    private void RebuildNotification()
    {
        AppNotificationBuilder builder = new AppNotificationBuilder()
            .AddArgument("action", "activate")
            .AddText(Title)
            .AddText(Message)
            .SetAttributionText(AttributionText)
            .SetScenario(NotificationScenario)
            .SetTimeStamp(TimeStamp)
            .SetDuration(Duration)
            .SetAudioEvent(SoundEvent);

        if (AppLogoUri != null)
        {
            _ = builder.SetAppLogoOverride(AppLogoUri);
        }

        if (HeroImagerUri != null)
        {
            _ = builder.SetHeroImage(HeroImagerUri);
        }

        NotificationToBuild = builder;
        notification = builder.BuildNotification();
    }

    public void ShowNotification()
    {
        RebuildNotification();
        AppNotificationManager.Default.Show(notification);
    }
}
