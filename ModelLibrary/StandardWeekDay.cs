using Microsoft.EntityFrameworkCore;
using Prism.Mvvm;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelLibrary
{
    public class StandardWeekContext : DbContext
    {
        public DbSet<StandardWeekDay> StandardWeek { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=RoomInfo.db");
        }
    }
    public class StandardWeekDay : BindableBase
    {
        int _id = default(int);
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get => _id; set { SetProperty(ref _id, value); } }
    }
}
