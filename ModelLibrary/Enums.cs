namespace ModelLibrary
{
    public enum OccupancyVisualState { FreeVisualState, PresentVisualState, AbsentVisualState, BusyVisualState, OccupiedVisualState, LockedVisualState, HomeVisualState, UndefinedVisualState }

    public enum PayloadType { Occupancy, Room, Schedule, StandardWeek, RequestOccupancy, RequestSchedule, RequestStandardWeek, IotDim, AgendaItem, AgendaItemId, Discovery, PropertyChanged, TimeSpanItem, TimeSpanItemId }

    public enum Language { de_DE, en_US }
}