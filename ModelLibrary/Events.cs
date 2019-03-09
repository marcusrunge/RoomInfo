﻿using Prism.Events;

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
    public class UpdateTimespanItemEvent : PubSubEvent<TimespanItem>
    {
    }
    public class DeleteTimespanItemEvent : PubSubEvent<object>
    {
    }
}
