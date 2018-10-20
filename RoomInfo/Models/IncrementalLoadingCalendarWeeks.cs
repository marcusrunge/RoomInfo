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
                int preOverlap = (int)dayOfWeek;
                int postOverlap = 7 - (int)dayOfWeek;
                if (preOverlap > 0)
                {
                    int j = 1;
                    for (int i = preOverlap; i > 0; i--)
                    {
                        if (j == 1) calendarWeek.WeekDayOneDate = today.Subtract(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 2) calendarWeek.WeekDayTwoDate = today.Subtract(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 3) calendarWeek.WeekDayThreeDate = today.Subtract(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 4) calendarWeek.WeekDayFourDate = today.Subtract(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 5) calendarWeek.WeekDayFiveDate = today.Subtract(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 6) calendarWeek.WeekDaySixDate = today.Subtract(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 7) calendarWeek.WeekDaySevenDate = today.Subtract(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        j++;
                    }
                }

                if ((int)dayOfWeek == 1) calendarWeek.WeekDayOneDate = today.Day.ToString();
                else if ((int)dayOfWeek == 2) calendarWeek.WeekDayTwoDate = today.Day.ToString();
                else if ((int)dayOfWeek == 3) calendarWeek.WeekDayThreeDate = today.Day.ToString();
                else if ((int)dayOfWeek == 4) calendarWeek.WeekDayFourDate = today.Day.ToString();
                else if ((int)dayOfWeek == 5) calendarWeek.WeekDayFiveDate = today.Day.ToString();
                else if ((int)dayOfWeek == 6) calendarWeek.WeekDaySixDate = today.Day.ToString();
                else if ((int)dayOfWeek == 7) calendarWeek.WeekDaySevenDate = today.Day.ToString();

                if (postOverlap > 0)
                {
                    int j = (int)dayOfWeek + 1;
                    for (int i = 1; i <= postOverlap; i++)
                    {
                        if (j == 1) calendarWeek.WeekDayOneDate = today.Add(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 2) calendarWeek.WeekDayTwoDate = today.Add(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 3) calendarWeek.WeekDayThreeDate = today.Add(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 4) calendarWeek.WeekDayFourDate = today.Add(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 5) calendarWeek.WeekDayFiveDate = today.Add(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 6) calendarWeek.WeekDaySixDate = today.Add(new TimeSpan(i, 0, 0, 0)).Day.ToString();
                        else if (j == 7) calendarWeek.WeekDaySevenDate = today.Add(new TimeSpan(i, 0, 0, 0)).Day.ToString();                        
                        j++;
                    }
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
