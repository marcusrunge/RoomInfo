using Microsoft.EntityFrameworkCore;
using ModelLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        Task<int> AddTimeSpanItemAsync(TimeSpanItem timeSpanItem);

        Task RemoveTimeSpanItemAsync(TimeSpanItem timeSpanItem);

        Task RemoveTimeSpanItemAsync(int id);

        Task UpdateTimeSpanItemAsync(TimeSpanItem timeSpanItem, bool remote = false);

        Task UpdateTimeSpanItemsAsync(List<TimeSpanItem> timeSpanItems, bool remote = false);

        Task<List<TimeSpanItem>> GetTimeSpanItemsAsync();
    }

    public class DatabaseService : IDatabaseService
    {
        private readonly ExceptionLogItemContext _exceptionLogItemContext;
        private readonly TimeSpanItemContext _timespanItemContext;
        private ManualResetEvent _manualResetEvent;

        public DatabaseService()
        {
            try
            {
                _exceptionLogItemContext = new ExceptionLogItemContext();
                _exceptionLogItemContext.Database.ExecuteSqlRaw($"CREATE TABLE IF NOT EXISTS ExceptionLogItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, TimeStamp NUMERIC, Message TEXT, Source TEXT, StackTrace TEXT)");
                _exceptionLogItemContext.Database.Migrate();
            }
            catch { }

            try
            {
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                agendaItemContext.Database.ExecuteSqlRaw($"CREATE TABLE IF NOT EXISTS AgendaItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT, Start NUMERIC , End NUMERIC , Description TEXT, IsAllDayEvent INTEGER, IsOverridden INTEGER, Occupancy INTEGER, TimeStamp NUMERIC, IsDeleted INTEGER)");
                agendaItemContext.Database.Migrate();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) Task.Run(async () => await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace }));
            }

            try
            {
                _timespanItemContext = new TimeSpanItemContext();
                _timespanItemContext.Database.ExecuteSqlRaw($"CREATE TABLE IF NOT EXISTS TimeSpanItems (Id INTEGER PRIMARY KEY AUTOINCREMENT, DayOfWeek INTEGER, Start NUMERIC , End NUMERIC, Occupancy INTEGER, TimeStamp NUMERIC, IsDeleted INTEGER)");
                _timespanItemContext.Database.Migrate();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) Task.Run(async () => await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace }));
            }
            _manualResetEvent = new ManualResetEvent(true);
        }

        public async Task<int> AddAgendaItemAsync(AgendaItem agendaItem)
        {
            _manualResetEvent.WaitOne();
            try
            {
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                agendaItem = (await agendaItemContext.AddAsync(agendaItem)).Entity;
                await agendaItemContext.SaveChangesAsync();
                _manualResetEvent.Set();
                return agendaItem.Id;
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                _manualResetEvent.Set();
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
                return agendaItemContext.AgendaItems.AsEnumerable()
                                .Where((x) => x.End.DateTime > dateTime)
                                .Select((x) => x)
                                .Take(3)
                                .OrderBy(x => x.Start)
                                .ToList();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                return new List<AgendaItem>();
            }
        }

        public async Task RemoveAgendaItemAsync(AgendaItem agendaItem)
        {
            _manualResetEvent.WaitOne();
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
            _manualResetEvent.Set();
        }

        public async Task RemoveAgendaItemAsync(int id)
        {
            _manualResetEvent.WaitOne();
            try
            {
                AgendaItemContext agendaItemContext = new AgendaItemContext();
                var agendaItem = await agendaItemContext.AgendaItems.Where(x => x.Id == id).Select(x => x).FirstOrDefaultAsync();
                if (agendaItem != null)
                {
                    agendaItemContext.Remove(agendaItem);
                    await agendaItemContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
            _manualResetEvent.Set();
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
            _manualResetEvent.WaitOne();
            if (agendaItem != null)
            {
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
                            agendaItemContext.Entry(queriedAgendaItem).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        agendaItemContext.Update(agendaItem);
                        agendaItemContext.Entry(agendaItem).State = EntityState.Modified;
                    }
                    bool saveFailed;
                    do
                    {
                        saveFailed = false;
                        try
                        {
                            await agendaItemContext.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            saveFailed = true;
                            var entry = ex.Entries.Single();
                            entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                        }
                    } while (saveFailed);
                }
                catch (Exception e)
                {
                    if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                }
            }
            _manualResetEvent.Set();
        }

        public async Task UpdateAgendaItemsAsync(List<AgendaItem> agendaItems, bool remote = false)
        {
            _manualResetEvent.WaitOne();
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
                bool saveFailed;
                do
                {
                    saveFailed = false;
                    try
                    {
                        await agendaItemContext.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        saveFailed = true;
                        var entry = ex.Entries.Single();
                        entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                    }
                } while (saveFailed);
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
            _manualResetEvent.Set();
        }

        public async Task<int> AddTimeSpanItemAsync(TimeSpanItem timeSpanItem)
        {
            _manualResetEvent.WaitOne();
            try
            {
                TimeSpanItemContext timeSpanItemContext = new TimeSpanItemContext();
                timeSpanItem = (await timeSpanItemContext.AddAsync(timeSpanItem)).Entity;
                await timeSpanItemContext.SaveChangesAsync();
                _manualResetEvent.Set();
                return timeSpanItem.Id;
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                _manualResetEvent.Set();
                return int.MinValue;
            }
        }

        public async Task RemoveTimeSpanItemAsync(TimeSpanItem timeSpanItem)
        {
            _manualResetEvent.WaitOne();
            try
            {
                TimeSpanItemContext timeSpanItemContext = new TimeSpanItemContext();
                timeSpanItemContext.Remove(timeSpanItem);
                await timeSpanItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
            _manualResetEvent.Set();
        }

        public async Task RemoveTimeSpanItemAsync(int id)
        {
            _manualResetEvent.WaitOne();
            try
            {
                TimeSpanItemContext timespanItemContext = new TimeSpanItemContext();
                var timespanItem = await timespanItemContext.TimeSpanItems.Where(x => x.Id == id).Select(x => x).FirstOrDefaultAsync();
                if (timespanItem != null)
                {
                    timespanItemContext.Remove(timespanItem);
                    await timespanItemContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
            _manualResetEvent.Set();
        }

        public async Task UpdateTimeSpanItemAsync(TimeSpanItem timeSpanItem, bool remote = false)
        {
            _manualResetEvent.WaitOne();
            if (timeSpanItem == null) return;
            try
            {
                TimeSpanItemContext timeSpanItemContext = new TimeSpanItemContext();
                timeSpanItemContext.Entry(timeSpanItem).State = EntityState.Modified;
                if (remote)
                {
                    var queriedTimespanItem = await timeSpanItemContext.TimeSpanItems.Where(x => x.Id == timeSpanItem.Id).Select(x => x).FirstOrDefaultAsync();
                    if (queriedTimespanItem != null)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            queriedTimespanItem.End = timeSpanItem.End;
                            queriedTimespanItem.Occupancy = timeSpanItem.Occupancy;
                            queriedTimespanItem.Start = timeSpanItem.Start;
                            queriedTimespanItem.TimeStamp = timeSpanItem.TimeStamp;
                        });
                        timeSpanItemContext.Update(queriedTimespanItem);
                    }
                }
                else timeSpanItemContext.Update(timeSpanItem);
                await timeSpanItemContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
            _manualResetEvent.Set();
        }

        public async Task UpdateTimeSpanItemsAsync(List<TimeSpanItem> timeSpanItems, bool remote = false)
        {
            _manualResetEvent.WaitOne();
            if (timeSpanItems == null) return;
            try
            {
                TimeSpanItemContext timeSpanItemContext = new TimeSpanItemContext();
                if (remote)
                {
                    timeSpanItems.ForEach(async x =>
                    {
                        if (x.Id == 0) await AddTimeSpanItemAsync(x);
                        else if (x.IsDeleted) await RemoveTimeSpanItemAsync(x);
                        else
                        {
                            var updatedTimeSpanItem = await timeSpanItemContext.TimeSpanItems.Where(y => y.Id == x.Id && y.TimeStamp != x.TimeStamp).FirstOrDefaultAsync();
                            if (updatedTimeSpanItem != null)
                            {
                                updatedTimeSpanItem.DayOfWeek = x.DayOfWeek;
                                updatedTimeSpanItem.End = x.End;
                                updatedTimeSpanItem.Occupancy = x.Occupancy;
                                updatedTimeSpanItem.Start = x.Start;
                                updatedTimeSpanItem.TimeStamp = x.TimeStamp;
                            }
                        }
                    });
                }
                else
                {
                    timeSpanItems.ForEach(async x =>
                    {
                        if (x.Id == 0) await AddTimeSpanItemAsync(x);
                        else if (x.IsDeleted) await RemoveTimeSpanItemAsync(x);
                        else
                        {
                            var updatedTimeSpanItem = await timeSpanItemContext.TimeSpanItems.Where(y => y.Id == x.Id && y.TimeStamp != x.TimeStamp).FirstOrDefaultAsync();
                            if (updatedTimeSpanItem != null) await UpdateTimeSpanItemAsync(updatedTimeSpanItem);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
            }
            _manualResetEvent.Set();
        }

        public async Task<List<TimeSpanItem>> GetTimeSpanItemsAsync()
        {
            _manualResetEvent.WaitOne();
            try
            {
                TimeSpanItemContext timeSpanItemContext = new TimeSpanItemContext();
                _manualResetEvent.Set();
                return await timeSpanItemContext.TimeSpanItems.ToListAsync();
            }
            catch (Exception e)
            {
                if (_exceptionLogItemContext != null) await AddExceptionLogItem(new ExceptionLogItem() { TimeStamp = DateTime.Now, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace });
                _manualResetEvent.Set();
                return new List<TimeSpanItem>();
            }
        }
    }
}