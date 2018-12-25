using Microsoft.EntityFrameworkCore;
using ModelComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseComponent
{
    public interface IDatabaseService
    {
        Task AddAgendaItemAsync(AgendaItem agendaItem);
        Task RemoveAgendaItemAsync(AgendaItem agendaItem);
        Task UpdateAgendaItemAsync(AgendaItem agendaItem);
        Task<List<AgendaItem>> GetAgendaItemsAsync();
        Task<List<AgendaItem>> GetAgendaItemsAsync(DateTime dateTime);
    }
    public class DatabaseService : IDatabaseService
    {
        AgendaItemContext _agendaItemContext;
        public DatabaseService()
        {            
            _agendaItemContext = new AgendaItemContext();
            //_agendaItemContext.Database.ExecuteSqlCommand("DROP TABLE AgendaItems");
            _agendaItemContext.Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS AgendaItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT, Start NUMERIC , End NUMERIC , Description TEXT, IsAllDayEvent INTEGER, IsOverridden INTEGER, Occupancy INTEGER)");
            //_agendaItemContext.Database.ExecuteSqlCommand("ALTER TABLE AgendaItems ADD COLUMN IsOverridden INTEGER");
            _agendaItemContext.Database.Migrate();
        }

        public async Task AddAgendaItemAsync(AgendaItem agendaItem)
        {
            await _agendaItemContext.AddAsync(agendaItem);
            await _agendaItemContext.SaveChangesAsync();
        }

        public async Task<List<AgendaItem>> GetAgendaItemsAsync()
        {
            return await _agendaItemContext.AgendaItems.ToListAsync();
        }

        public async Task<List<AgendaItem>> GetAgendaItemsAsync(DateTime dateTime)
        {
            return await _agendaItemContext.AgendaItems
                .Where((x) => x.End.DateTime > dateTime)
                .Select((x) => x)
                .Take(3)
                .ToListAsync();
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
}
