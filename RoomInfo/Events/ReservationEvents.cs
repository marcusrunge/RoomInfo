using Prism.Events;
using RoomInfo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomInfo.Events
{
    public class DeleteReservationEvent : PubSubEvent<object>
    {
    }
    public class UpdateReservationEvent : PubSubEvent<AgendaItem>
    {
    }
}
