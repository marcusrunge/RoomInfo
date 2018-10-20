using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace RoomInfo.Models
{
    public class IncrementalLoadingCalendarWeeks : ObservableCollection<CalendarWeek>, ISupportIncrementalLoading
    {
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return AsyncInfo.Run(async cancellationToken =>
            {
                for (int i = 0; i < count; i++)
                {
                    CalendarWeek firstCalendarWeek = this.First();
                    Add(new CalendarWeek()
                    {
                        
                    });
                }
                return new LoadMoreItemsResult { Count = count };
            });
        }
                
        public bool HasMoreItems => 0 < 10000;
    }
}
