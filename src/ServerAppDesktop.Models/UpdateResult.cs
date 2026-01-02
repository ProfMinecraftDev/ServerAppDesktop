namespace ServerAppDesktop.Models
{
    public class UpdateResult
    {
        public bool IsNewVersion { get; } = false;
        public string CurrentVersion { get; } = "";
        public string LatestVersion { get; } = "";
        public string Notes { get; } = "";
        public string ExeUrl { get; } = "";
        public bool IsPreRelease { get; } = false;

        public UpdateResult(bool isNewVersion, string currentVersion, string latestVersion, string notes, string exeUrl, bool isPreRelease)
        {
            IsNewVersion = isNewVersion;
            CurrentVersion = currentVersion;
            LatestVersion = latestVersion;
            Notes = notes;
            ExeUrl = exeUrl;
            IsPreRelease = isPreRelease;
        }
    }
}
