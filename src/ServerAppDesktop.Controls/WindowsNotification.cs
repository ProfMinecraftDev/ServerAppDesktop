using System;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace ServerAppDesktop.Controls
{
    public sealed class WindowsNotification
    {
        private AppNotification? notification;
        private AppNotificationBuilder? _notificationToBuild;

        private AppNotificationScenario _scenario = AppNotificationScenario.Default;
        public AppNotificationScenario NotificationScenario
        {
            get => _scenario;
            set { _scenario = value; RebuildNotification(); }
        }

        private Uri? _appLogoUri;
        public Uri? AppLogoUri
        {
            get => _appLogoUri;
            set { _appLogoUri = value; RebuildNotification(); }
        }

        private Uri? _heroImageUri;
        public Uri? HeroImagerUri
        {
            get => _heroImageUri;
            set { _heroImageUri = value; RebuildNotification(); }
        }

        private string _title = "";
        public string Title
        {
            get => _title;
            set { _title = value; RebuildNotification(); }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set { _message = value; RebuildNotification(); }
        }

        private string _attributionText = "";
        public string AttributionText
        {
            get => _attributionText;
            set { _attributionText = value; RebuildNotification(); }
        }

        private AppNotificationSoundEvent _soundEvent = AppNotificationSoundEvent.Default;
        public AppNotificationSoundEvent SoundEvent
        {
            get => _soundEvent;
            set { _soundEvent = value; RebuildNotification(); }
        }

        private AppNotificationDuration _duration = AppNotificationDuration.Default;
        public AppNotificationDuration Duration
        {
            get => _duration;
            set { _duration = value; RebuildNotification(); }
        }

        private DateTime _timeStamp = DateTime.Now;
        public DateTime TimeStamp
        {
            get => _timeStamp;
            set { _timeStamp = value; RebuildNotification(); }
        }

        public AppNotification Notification => notification ?? throw new InvalidOperationException("La notificación no ha sido construida.");
        public AppNotificationBuilder NotificationToBuild => _notificationToBuild ?? throw new InvalidOperationException("La notificación no ha sido construida.");

        private void RebuildNotification()
        {
            var builder = new AppNotificationBuilder()
                .AddArgument("action", "activate")
                .AddText(Title)
                .AddText(Message)
                .SetAttributionText(AttributionText)
                .SetScenario(NotificationScenario)
                .SetTimeStamp(TimeStamp)
                .SetDuration(Duration)
                .SetAudioEvent(SoundEvent);

            if (AppLogoUri != null)
                builder.SetAppLogoOverride(AppLogoUri);

            if (HeroImagerUri != null)
                builder.SetHeroImage(HeroImagerUri);

            _notificationToBuild = builder;
            notification = builder.BuildNotification();
        }

        public void ShowNotification()
        {
            RebuildNotification();
            AppNotificationManager.Default.Show(notification);
        }
    }
}
