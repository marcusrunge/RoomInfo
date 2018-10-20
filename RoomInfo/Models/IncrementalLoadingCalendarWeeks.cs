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
                    
                    Add(CalculateCalendarWeek(firstCalendarWeek));
                }
                return new LoadMoreItemsResult { Count = count };
            });
        }

        private CalendarWeek CalculateCalendarWeek(CalendarWeek firstCalendarWeek)
        {
            CalendarWeek calendarWeek = new CalendarWeek
            {
                WeekDayOne = new ObservableCollection<AgendaItem>(),
                WeekDayTwo = new ObservableCollection<AgendaItem>(),
                WeekDayThree = new ObservableCollection<AgendaItem>(),
                WeekDayFour = new ObservableCollection<AgendaItem>(),
                WeekDayFive = new ObservableCollection<AgendaItem>(),
                WeekDaySix = new ObservableCollection<AgendaItem>(),
                WeekDaySeven = new ObservableCollection<AgendaItem>()
            };
            if (firstCalendarWeek == null)
            {
                DateTime today = DateTime.Today;
                int month = today.Month;
                int year = today.Year;
                int day = today.Day;
                DayOfWeek dayOfWeek = today.DayOfWeek;
                int preOverlap = (day - (int)dayOfWeek) * -1;                
                if (preOverlap > 0)
                {                    
                    for (int i = preOverlap; i > 0; i--)
                    {
                        calendarWeek.WeekDayOneDate = today.Subtract(new TimeSpan(i,0,0,0)).Day.ToString();
                    }
                }
                else
                {

                }
            }
            else
            {

            }
            return calendarWeek;
        }

        public bool HasMoreItems => 0 < 10000;
    }
}
