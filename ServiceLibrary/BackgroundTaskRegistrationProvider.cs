using Windows.ApplicationModel.Background;

namespace ApplicationServiceLibrary
{
    public interface IBackgroundTaskRegistrationProvider
    {
        BackgroundTaskRegistration BackgroundTaskRegistration { get; set; }
    }
    public class BackgroundTaskRegistrationProvider : IBackgroundTaskRegistrationProvider
    {
        public BackgroundTaskRegistration BackgroundTaskRegistration { get; set; }
    }
}
