using Microsoft.EntityFrameworkCore;
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
        Task<int> AddTimespanItemAsync(TimespanItem timespanItem);
        Task RemoveTimespanItemAsync(TimespanItem timespanItem);
        Task RemoveTimespanItemAsync(int id);
        Task UpdateTimespanItemAsync(TimespanItem timespanItem, bool remote = false);
        Task UpdateTimespanItemsAsync(List<TimespanItem> timespanItems, bool remote = false);
        Task<List<TimespanItem>> GetTimespanItemsAsync();
    }
    public class DatabaseService : IDatabaseService
    {
        readonly ExceptionLogItemContext _exceptionLogItemContext;
        readonly TimespanItemContext _timespanItemContext;
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
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                agendaItemContext.Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS AgendaItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT, Start NUMERIC , End NUMERIC , Description TEXT, IsAllDayEvent INTEGER, IsOverridden INTEGER, Occupancy INTEGER, TimeStamp NUMERIC, IsDeleted INTEGER)");
                agendaItemContext.Database.Migrate();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) Task.Run(async () => await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace }));
            }

            try
            {
                _timespanItemContext = new TimespanItemContext();
                _timespanItemContext.Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS TimespanItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, DayOfWeek INTEGER, Start NUMERIC , End NUMERIC, Occupancy INTEGER, TimeStamp NUMERIC, IsDeleted INTEGER)");
                _timespanItemContext.Database.Migrate();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) Task.Run(async () => await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace }));
            }
        }

        public async Task<int> AddAgendaItemAsync(AgendaItem agendaItem)
        {
            try
            {
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                agendaItem = (await agendaItemContext.AddAsync(agendaItem)).Entity;
                await agendaItemContext.SaveChangesAsync();
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
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                return await agendaItemContext.AgendaItems.ToListAsync();
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
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                return await agendaItemContext.AgendaItems
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
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                agendaItemContext.Remove(agendaItem);
                await agendaItemContext.SaveChangesAsync();
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
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                var agendaItem = await agendaItemContext.AgendaItems.Where(x => x.Id == id).Select(x => x).FirstOrDefaultAsync();
                agendaItemContext.Remove(agendaItem);
                await agendaItemContext.SaveChangesAsync();
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
            if (agendaItem == null) return;
            try
            {
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                agendaItemContext.Entry(agendaItem).State = EntityState.Modified;
                if (remote)
                {
                    var queriedAgendaItem = await agendaItemContext.AgendaItems.Where(x => x.Id == agendaItem.Id).Select(x => x).FirstOrDefaultAsync();
                    if (queriedAgendaItem != null)
                    {
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
                        agendaItemContext.Update(queriedAgendaItem);
                    }
                }
                else agendaItemContext.Update(agendaItem);
                await agendaItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        public async Task UpdateAgendaItemsAsync(List<AgendaItem> agendaItems, bool remote = false)
        {
            if (agendaItems == null) return;
            try
            {
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                if (remote)
                {
                    agendaItems.ForEach(async x =>
                    {
                        if (x.Id == 0) await AddAgendaItemAsync(x);
                        else if (x.IsDeleted) await RemoveAgendaItemAsync(x);
                        else
                        {
                            var updatedAgendaItem = await agendaItemContext.AgendaItems.Where(y => y.Id == x.Id && y.TimeStamp != x.TimeStamp).FirstOrDefaultAsync();
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
                                agendaItemContext.Entry(updatedAgendaItem).State = EntityState.Modified;
                                agendaItemContext.Update(updatedAgendaItem);
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
                            var updatedAgendaItem = await agendaItemContext.AgendaItems.Where(y => y.Id == x.Id && y.TimeStamp != x.TimeStamp).FirstOrDefaultAsync();
                            if (updatedAgendaItem != null) await UpdateAgendaItemAsync(updatedAgendaItem);
                        }
                    });
                }
                await agendaItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        public async Task<int> AddTimespanItemAsync(TimespanItem timespanItem)
        {
            try
            {
                TimespanItemContext timespanItemContext = new TimespanItemContext();
                timespanItem = (await timespanItemContext.AddAsync(timespanItem)).Entity;
                await timespanItemContext.SaveChangesAsync();
                return timespanItem.Id;
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                return int.MinValue;
            }
        }

        public async Task RemoveTimespanItemAsync(TimespanItem timespanItem)
        {
            try
            {
                TimespanItemContext timespanItemContext = new TimespanItemContext();
                timespanItemContext.Remove(timespanItem);
                await timespanItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        public async Task RemoveTimespanItemAsync(int id)
        {
            try
            {
                TimespanItemContext timespanItemContext = new TimespanItemContext();
                var timespanItem = await timespanItemContext.TimespanItems.Where(x => x.Id == id).Select(x => x).FirstOrDefaultAsync();
                timespanItemContext.Remove(timespanItem);
                await timespanItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        public async Task UpdateTimespanItemAsync(TimespanItem timespanItem, bool remote = false)
        {
            if (timespanItem == null) return;
            try
            {
                TimespanItemContext timespanItemContext = new TimespanItemContext();
                timespanItemContext.Entry(timespanItem).State = EntityState.Modified;
                if (remote)
                {
                    var queriedTimespanItem = await timespanItemContext.TimespanItems.Where(x => x.Id == timespanItem.Id).Select(x => x).FirstOrDefaultAsync();
                    if (queriedTimespanItem != null)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            queriedTimespanItem.End = timespanItem.End;                            
                            queriedTimespanItem.Occupancy = timespanItem.Occupancy;
                            queriedTimespanItem.Start = timespanItem.Start;
                            queriedTimespanItem.TimeStamp = timespanItem.TimeStamp;
                        });
                        timespanItemContext.Update(queriedTimespanItem);
                    }
                }
                else timespanItemContext.Update(timespanItem);
                await timespanItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
        }

        public async Task UpdateTimespanItemsAsync(List<TimespanItem> timespanItems, bool remote = false)
        {
            try
            {
                TimespanItemContext timespanItemContext = new TimespanItemContext();
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }            
        }

        public async Task<List<TimespanItem>> GetTimespanItemsAsync()
        {
            try
            {
                TimespanItemContext timespanItemContext = new TimespanItemContext();
                return await timespanItemContext.TimespanItems.ToListAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                return new List<TimespanItem>();
            }
        }
    }
}
