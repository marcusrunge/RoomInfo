using Windows.ApplicationModel.Background;

namespace BackgroundComponent
{
    public sealed class LiveTileUpdateBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            _deferral.Complete();
        }
    }
}
