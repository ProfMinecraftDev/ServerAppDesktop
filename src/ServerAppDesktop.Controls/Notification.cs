using System;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace ServerAppDesktop.Controls
{
    public sealed class WindowsNotification
    {
        private AppNotification? notification;

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
        public string Messsage
        {
            get => _message;
            set { _message = value; RebuildNotification(); }
        }

        private string _attributionText = "";
        public string AttributonText
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

        private void RebuildNotification()
        {
            var builder = new AppNotificationBuilder()
                .AddText(Title)
                .AddText(Messsage)
                .SetAttributionText(AttributonText)
                .SetScenario(NotificationScenario)
                .SetTimeStamp(TimeStamp)
                .SetDuration(Duration)
                .SetAudioEvent(SoundEvent);

            if (AppLogoUri != null)
                builder.SetAppLogoOverride(AppLogoUri);

            if (HeroImagerUri != null)
                builder.SetHeroImage(HeroImagerUri);

            notification = builder.BuildNotification();
        }

        public void ShowNotification()
        {
            RebuildNotification();
            AppNotificationManager.Default.Show(notification);
        }
    }
}
