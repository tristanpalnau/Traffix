using Traffix.Events;

namespace Traffix.Core;

/// <summary>
/// Maintains a chronologically ordered list of pending simulation events.
/// Events are sorted by <see cref="SimulationEvent.Time"/> on every insertion,
/// ensuring <see cref="GetNextEvent"/> always returns the earliest event.
/// </summary>
public class EventQueue
{
    private List<SimulationEvent> _events;

    public EventQueue()
    {
        _events = new List<SimulationEvent>();
    }

    /// <summary>Adds an event and re-sorts the queue by time.</summary>
    public void AddEvent(SimulationEvent simEvent)
    {
        _events.Add(simEvent);

        _events = _events
        .OrderBy(e => e.Time)
        .ToList();
    }

    /// <summary>Removes and returns the earliest-scheduled event.</summary>
    public SimulationEvent GetNextEvent()
    {
        SimulationEvent nextEvent = _events[0];

        _events.RemoveAt(0);

        return nextEvent;
    }

    /// <summary>Returns <c>true</c> if there are events remaining to process.</summary>
    public bool HasEvents()
    {
        return _events.Count() > 0;
    }
}