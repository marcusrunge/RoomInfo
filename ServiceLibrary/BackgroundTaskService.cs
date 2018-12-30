using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace ApplicationServiceLibrary
{
    public interface IBackgroundTaskService
    {
        IBackgroundTaskRegistration FindRegistration<T>() where T : class;
        Task<BackgroundTaskRegistration> Register<T>(IBackgroundTrigger trigger, IEnumerable<IBackgroundCondition> conditions = null) where T : class;
        Task<bool> Unregister<T>() where T : class;
    }
    public class BackgroundTaskService : IBackgroundTaskService
    {
        public IBackgroundTaskRegistration FindRegistration<T>() where T : class
        {
            return BackgroundTaskRegistration.AllTasks
                .Where(x => x.Value.Name.Equals(typeof(T).Name))
                .Select(x => x.Value)
                .FirstOrDefault();
        }

        public async Task<BackgroundTaskRegistration> Register<T>(IBackgroundTrigger trigger, IEnumerable<IBackgroundCondition> conditions = null) where T : class
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            var allowed = BackgroundExecutionManager.GetAccessStatus();
            switch (allowed)
            {
                case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                case BackgroundAccessStatus.AlwaysAllowed:
                    break;
                case BackgroundAccessStatus.Unspecified:
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                case BackgroundAccessStatus.DeniedByUser:
                    return null;
            }

            var existing = FindRegistration<T>();
            if (existing != null)
                existing.Unregister(false);

            var task = new BackgroundTaskBuilder
            {
                Name = typeof(T).Name,
                CancelOnConditionLoss = false,
                TaskEntryPoint = typeof(T).ToString(),
            };

            task.SetTrigger(trigger);
            if (conditions != null)
            {
                foreach (var condition in conditions)
                    task.AddCondition(condition);
            }
            return task.Register();
        }

        public async Task<bool> Unregister<T>() where T : class
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            var allowed = BackgroundExecutionManager.GetAccessStatus();
            switch (allowed)
            {
                case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                case BackgroundAccessStatus.AlwaysAllowed:
                    break;
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                case BackgroundAccessStatus.DeniedByUser:
                    return false;
            }

            var existing = FindRegistration<T>();
            if (existing != null)
                existing.Unregister(true);
            return true;
        }

        public async Task<bool> UnregisterAll()
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            var allowed = BackgroundExecutionManager.GetAccessStatus();
            switch (allowed)
            {
                case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                case BackgroundAccessStatus.AlwaysAllowed:
                    break;
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                case BackgroundAccessStatus.DeniedByUser:
                    return false;
            }
            var allTasks = BackgroundTaskRegistration.AllTasks;
            foreach (var item in allTasks)
            {
                item.Value.Unregister(false);
            }
            return true;
        }
    }
}
