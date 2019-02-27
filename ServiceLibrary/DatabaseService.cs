﻿using Microsoft.EntityFrameworkCore;
using ModelLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace ApplicationServiceLibrary
{
    public interface IDatabaseService
    {
        Task<int> AddAgendaItemAsync(AgendaItem agendaItem);
        Task RemoveAgendaItemAsync(AgendaItem agendaItem);
        Task RemoveAgendaItemAsync(int id);
        Task UpdateAgendaItemAsync(AgendaItem agendaItem, bool remote = false);
        Task UpdateAgendaItemsAsync(List<AgendaItem> agendaItems, bool remote = false);
        Task<List<AgendaItem>> GetAgendaItemsAsync();
        Task<List<AgendaItem>> GetAgendaItemsAsync(DateTime dateTime);
        Task<List<ExceptionLogItem>> GetExceptionLogItemsAsync();
        Task<int> AddExceptionLogItem(ExceptionLogItem exceptionLogItem);
        Task RemoveExceptionLogItemsAsync();
    }
    public class DatabaseService : IDatabaseService
    {
        static AgendaItemContext _agendaItemContext;
        static ExceptionLogItemContext _exceptionLogItemContext;
        static StandardWeekContext _standardWeekContext;
        public DatabaseService()
        {
            try
            {
                _exceptionLogItemContext = new ExceptionLogItemContext();
                _exceptionLogItemContext.Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS ExceptionLogItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, TimeStamp NUMERIC, Message TEXT, Source TEXT, StackTrace TEXT)");
                _exceptionLogItemContext.Database.Migrate();
            }
            catch { }

            try
            {
                _agendaItemContext = new AgendaItemContext();
                _agendaItemContext.Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS AgendaItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT, Start NUMERIC , End NUMERIC , Description TEXT, IsAllDayEvent INTEGER, IsOverridden INTEGER, Occupancy INTEGER, TimeStamp NUMERIC, IsDeleted INTEGER)");
                _agendaItemContext.Database.Migrate();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }

            try
            {
                //_standardWeekContext = new StandardWeekContext();
                //_standardWeekContext.Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS StandardWeek (Id INTEGER PRIMARY KEY AUTOINCREMENT)");
                //_standardWeekContext.Database.Migrate();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        public async Task<int> AddAgendaItemAsync(AgendaItem agendaItem)
        {
            try
            {
                agendaItem = (await _agendaItemContext.AddAsync(agendaItem)).Entity;
                await _agendaItemContext.SaveChangesAsync();
                return agendaItem.Id;
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                return int.MinValue;
            }
        }

        public async Task<int> AddExceptionLogItem(ExceptionLogItem exceptionLogItem)
        {
            try
            {
                exceptionLogItem = (await _exceptionLogItemContext.AddAsync(exceptionLogItem)).Entity;
                await _exceptionLogItemContext.SaveChangesAsync();
                return exceptionLogItem.Id;
            }
            catch (Exception)
            {
                return int.MinValue;
            }
        }

        public async Task<List<AgendaItem>> GetAgendaItemsAsync()
        {
            try
            {
                return await _agendaItemContext.AgendaItems.ToListAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                return new List<AgendaItem>();
            }
        }

        public async Task<List<ExceptionLogItem>> GetExceptionLogItemsAsync()
        {
            try
            {
                return await _exceptionLogItemContext.ExceptionLogItems.ToListAsync();
            }
            catch (Exception)
            {
                return new List<ExceptionLogItem>();
            }
        }

        public async Task<List<AgendaItem>> GetAgendaItemsAsync(DateTime dateTime)
        {
            try
            {
                return await _agendaItemContext.AgendaItems
                                .Where((x) => x.End.DateTime > dateTime)
                                .Select((x) => x)
                                .Take(3)
                                .ToListAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                return new List<AgendaItem>();
            }
        }

        public async Task RemoveAgendaItemAsync(AgendaItem agendaItem)
        {
            try
            {
                _agendaItemContext.Remove(agendaItem);
                await _agendaItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        public async Task RemoveAgendaItemAsync(int id)
        {
            try
            {
                var agendaItem = await _agendaItemContext.AgendaItems.Where(x => x.Id == id).Select(x => x).FirstOrDefaultAsync();
                _agendaItemContext.Remove(agendaItem);
                await _agendaItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        public async Task RemoveExceptionLogItemsAsync()
        {
            try
            {
                _exceptionLogItemContext.RemoveRange(_exceptionLogItemContext.ExceptionLogItems);
                await _exceptionLogItemContext.SaveChangesAsync();
            }
            catch { }
        }

        public async Task UpdateAgendaItemAsync(AgendaItem agendaItem, bool remote = false)
        {
            try
            {
                if (remote)
                {
                    var queriedAgendaItem = await _agendaItemContext.AgendaItems.Where(x => x.Id == agendaItem.Id).Select(x => x).FirstOrDefaultAsync();
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        queriedAgendaItem.Description = agendaItem.Description;
                        queriedAgendaItem.End = agendaItem.End;
                        queriedAgendaItem.IsAllDayEvent = agendaItem.IsAllDayEvent;
                        queriedAgendaItem.IsDeleted = agendaItem.IsDeleted;
                        queriedAgendaItem.IsOverridden = agendaItem.IsOverridden;
                        queriedAgendaItem.Occupancy = agendaItem.Occupancy;
                        queriedAgendaItem.Start = agendaItem.Start;
                        queriedAgendaItem.TimeStamp = agendaItem.TimeStamp;
                        queriedAgendaItem.Title = agendaItem.Title;
                    });
                    _agendaItemContext.Update(queriedAgendaItem);
                }
                else _agendaItemContext.Update(agendaItem);
                await _agendaItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }            
        }

        public async Task UpdateAgendaItemsAsync(List<AgendaItem> agendaItems, bool remote = false)
        {
            try
            {
                if (remote)
                {
                    agendaItems.ForEach(async x =>
                    {
                        if (x.Id == 0) await AddAgendaItemAsync(x);
                        else if (x.IsDeleted) await RemoveAgendaItemAsync(x);
                        else
                        {
                            var updatedAgendaItem = await _agendaItemContext.AgendaItems.Where(y => y.Id == x.Id && y.TimeStamp != x.TimeStamp).FirstOrDefaultAsync();
                            if (updatedAgendaItem != null)
                            {
                                updatedAgendaItem.Description = x.Description;
                                updatedAgendaItem.End = x.End;
                                updatedAgendaItem.IsAllDayEvent = x.IsAllDayEvent;
                                updatedAgendaItem.IsDeleted = x.IsDeleted;
                                updatedAgendaItem.IsOverridden = x.IsOverridden;
                                updatedAgendaItem.Occupancy = x.Occupancy;
                                updatedAgendaItem.Start = x.Start;
                                updatedAgendaItem.TimeStamp = x.TimeStamp;
                                updatedAgendaItem.Title = x.Title;
                                _agendaItemContext.Update(updatedAgendaItem);
                            }
                        }
                    });
                }
                else
                {
                    agendaItems.ForEach(async x =>
                    {
                        if (x.Id == 0) await AddAgendaItemAsync(x);
                        else if (x.IsDeleted) await RemoveAgendaItemAsync(x);
                        else
                        {
                            var updatedAgendaItem = await _agendaItemContext.AgendaItems.Where(y => y.Id == x.Id && y.TimeStamp != x.TimeStamp).FirstOrDefaultAsync();
                            if (updatedAgendaItem != null) await UpdateAgendaItemAsync(updatedAgendaItem);
                        }
                    });
                }
                await _agendaItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }            
        }
    }
}
