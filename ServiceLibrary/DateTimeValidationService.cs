using ModelLibrary;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationServiceLibrary
{
    public interface IDateTimeValidationService
    {
        bool Validate(AgendaItem agendaItem, List<AgendaItem> agendaItems);
    }
    public class DateTimeValidationService : IDateTimeValidationService
    {
        public bool Validate(AgendaItem agendaItem, List<AgendaItem> agendaItems)
        {
            return agendaItem.End >= agendaItem.Start
                ? agendaItems.Where(x => x.Id != agendaItem.Id && ((agendaItem.Start >= x.Start && agendaItem.Start <= x.End) || (agendaItem.End >= x.Start && agendaItem.End <= x.End))).FirstOrDefault() == null
                : false;
        }
    }
}
