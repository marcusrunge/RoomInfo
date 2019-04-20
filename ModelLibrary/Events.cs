using Prism.Events;
using Windows.UI.Xaml;

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
    public class RemoteOccupancyOverrideEvent : PubSubEvent<int>
    {
    }
    public class RemoteAgendaItemsUpdatedEvent : PubSubEvent
    {
    }
    public class FileItemSelectionChangedUpdatedEvent : PubSubEvent<int>
    {
    }
    public class WiFiNetworkSelectionChangedUpdatedEvent : PubSubEvent<int>
    {
    }
    public class WiFiNetworkConnectionChangedUpdatedEvent : PubSubEvent<int>
    {
    }
    public class CollapseLowerGridEvent : PubSubEvent
    {
    }
    public class PortChangedEvent : PubSubEvent
    {
    }
    public class RemoteAgendaItemDeletedEvent : PubSubEvent<int>
    {
    }
    public class UpdateTimespanItemEvent : PubSubEvent<TimeSpanItem>
    {
    }
    public class DeleteTimespanItemEvent : PubSubEvent<object>
    {
    }
    public class GotFocusEvent : PubSubEvent<FrameworkElement>
    {
    }
    public class StandardWeekUpdatedEvent : PubSubEvent<int>
    {
    }    
}
