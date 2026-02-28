namespace ServerAppDesktop.Messaging;

public class AppSettingsChangedMessage(AppSettings value) : ValueChangedMessage<AppSettings>(value)
{
}
