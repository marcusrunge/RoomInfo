﻿using Microsoft.EntityFrameworkCore;
using RoomInfo.Models;
using System;
using System.Collections.Generic;
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
            _agendaItemContext.Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS AgendaItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title NVARCHAR(30), DateTime NVARCHAR(10))");
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
