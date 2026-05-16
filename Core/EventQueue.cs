using Traffix.Events;

namespace Traffix.Core;

public class EventQueue
{
    private List<SimulationEvent> _events;

    public EventQueue()
    {
        _events = new List<SimulationEvent>();
    }

    public void AddEvent(SimulationEvent simEvent) 
    {
        _events.Add(simEvent);

        _events = _events
        .OrderBy(e => e.Time)
        .ToList();
    }

    public SimulationEvent GetNextEvent()
    {
        SimulationEvent nextEvent = _events[0];

        _events.RemoveAt(0);

        return nextEvent;
    }

    public bool HasEvents()
    {
        return _events.Count() > 0;
    }
}