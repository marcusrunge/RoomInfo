using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using RoomInfo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomInfo.Services
{
    public interface IDatabaseService
    {
        Task AddAgendaItemAsync(AgendaItem agendaItem);
        Task RemoveAgendaItemAsync(AgendaItem agendaItem);
        Task UpdateAgendaItemAsync(AgendaItem agendaItem);
        Task<List<AgendaItem>> GetAgendaItemsAsync();
    }
    public class DatabaseService : IDatabaseService
    {
        AgendaItemContext _agendaItemContext;
        public DatabaseService()
        {            
            _agendaItemContext = new AgendaItemContext();
            _agendaItemContext.Database.Migrate();
        }

        public async Task AddAgendaItemAsync(AgendaItem agendaItem)
        {
            await _agendaItemContext.AddAsync(agendaItem);
            //await _agendaItemContext.SaveChangesAsync();
        }

        public async Task<List<AgendaItem>> GetAgendaItemsAsync()
        {
            return await _agendaItemContext.AgendaItems.ToListAsync();
        }

        public async Task RemoveAgendaItemAsync(AgendaItem agendaItem)
        {
            _agendaItemContext.Remove(agendaItem);
            await _agendaItemContext.SaveChangesAsync();
        }

        public async Task UpdateAgendaItemAsync(AgendaItem agendaItem)
        {
            _agendaItemContext.Update(agendaItem);
            await _agendaItemContext.SaveChangesAsync();
        }
    }

    //public partial class CreateDatabase : Migration
    //{
    //    protected override void Up(MigrationBuilder migrationBuilder)
    //    {
    //        migrationBuilder.CreateTable(
    //            name: "AgendaItems",
    //            columns: table => new
    //            {
    //                Id = table.Column<int>(nullable: false).Annotation("MySQL:ValueGeneratedOnAdd", true),
    //                Title = table.Column<string>(nullable: true),
    //                DateTime = table.Column<string>(nullable: true)
    //            },
    //            constraints: table =>
    //            {
    //                table.PrimaryKey("PK_AgendaItems_Id", x => x.Id);
    //            });
    //    }
    //}
}
