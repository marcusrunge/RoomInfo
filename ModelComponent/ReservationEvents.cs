using Prism.Events;

namespace ModelComponent
{
    public class DeleteReservationEvent : PubSubEvent<object>
    {
    }
    public class UpdateReservationEvent : PubSubEvent<AgendaItem>
    {
    }
}
