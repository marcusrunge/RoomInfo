using ModelLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoomInfo.Helpers
{
    public class TimeValidator
    {
        public static TimeSpan ValidateStartTime(TimeSpanItem timeSpanItem, List<TimeSpanItem> timeSpanItems)
        {
            var collisioningTimeSpanItem = timeSpanItems.Where(x => timeSpanItem.Start >= x.Start && timeSpanItem.End <= x.End).Select(x => x).FirstOrDefault();
            return collisioningTimeSpanItem != null ? collisioningTimeSpanItem.End.Add(TimeSpan.FromMinutes(1)) : TimeSpan.Zero;
        }

        public static TimeSpan ValidateEndTime(TimeSpanItem timeSpanItem, List<TimeSpanItem> timeSpanItems)
        {
            if (timeSpanItem.End < timeSpanItem.Start) return timeSpanItem.Start;
            else
            {
                var collisioningTimeSpanItem = timeSpanItems.Where(x => timeSpanItem.Start >= x.Start && timeSpanItem.End <= x.End).Select(x => x).FirstOrDefault();
                return collisioningTimeSpanItem != null ? collisioningTimeSpanItem.Start.Subtract(TimeSpan.FromMinutes(1)) : TimeSpan.Zero;
            }
        }
    }
}
