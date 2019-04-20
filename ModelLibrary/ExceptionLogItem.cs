using Microsoft.EntityFrameworkCore;
using Prism.Mvvm;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelLibrary
{
    public class ExceptionLogItemContext : DbContext
    {
        public DbSet<ExceptionLogItem> ExceptionLogItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=RoomInfo.db");
        }
    }

    public class ExceptionLogItem : BindableBase
    {
        int _id = default;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get => _id; set { SetProperty(ref _id, value); } }

        DateTime _timeStamp = default;
        public DateTime TimeStamp { get => _timeStamp; set { SetProperty(ref _timeStamp, value); } }

        string _message = default;
        public string Message { get => _message; set { SetProperty(ref _message, value); } }

        string _source = default;
        public string Source { get => _source; set { SetProperty(ref _source, value); } }

        string _stackTrace = default;
        public string StackTrace { get => _stackTrace; set { SetProperty(ref _stackTrace, value); } }
    }
}
