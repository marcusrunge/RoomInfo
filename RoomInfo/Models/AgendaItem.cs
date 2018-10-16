using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomInfo.Models
{
    public class AgendaItemContext : DbContext
    {
        public DbSet<AgendaItem> AgendaItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=AgendaItems.db");
        }
    }

    public class AgendaItem : DataModelBase
    {
        int _id = default(int);
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get => _id; set { SetProperty(ref _id, value); } }

        string _title = default(string);
        public string Title { get => _title; set { SetProperty(ref _title, value); } }

        DateTimeOffset _startDate = default(DateTimeOffset);
        public DateTimeOffset StartDate { get => _startDate; set { SetProperty(ref _startDate, value); } }

        DateTimeOffset _endDate = default(DateTimeOffset);
        public DateTimeOffset EndDate { get => _endDate; set { SetProperty(ref _endDate, value); } }

        TimeSpan _startTime = default(TimeSpan);
        public TimeSpan StartTime { get => _startTime; set { SetProperty(ref _startTime, value); } }

        TimeSpan _endTime = default(TimeSpan);
        public TimeSpan EndTime { get => _endTime; set { SetProperty(ref _endTime, value); } }

        bool _isAllDayEvent = default(bool);
        public bool IsAllDayEvent { get => _isAllDayEvent; set { SetProperty(ref _isAllDayEvent, value); } }               

        string _description = default(string);
        public string Description { get => _description; set { SetProperty(ref _description, value); } }
    }
}
