using Prism.Events;

namespace ModelLibrary
{
    public class DeleteReservationEvent : PubSubEvent<object>
    {
    }
    public class UpdateReservationEvent : PubSubEvent<AgendaItem>
    {
    }
    public class UpdateWidthEvent : PubSubEvent<double>
    {
    }
}
