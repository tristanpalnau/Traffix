using Traffix.Entities;

namespace Traffix.Events;

/// <summary>
/// A scheduled occurrence in the simulation. Events are placed in the
/// <see cref="Traffix.Core.EventQueue"/> and processed in ascending <see cref="Time"/> order.
/// </summary>
public class SimulationEvent
{
    /// <summary>Simulation time (minutes) at which this event occurs.</summary>
    public double Time { get; set; }

    /// <summary>The kind of occurrence this event represents.</summary>
    public EventType EventType { get; set; }

    /// <summary>The party this event is associated with.</summary>
    public Party Party { get; set; }

    /// <summary>
    /// The table involved in this event, or <c>null</c> for arrival events
    /// where a table has not yet been assigned.
    /// </summary>
    public Table? Table { get; set; }

    public SimulationEvent(
        double time,
        EventType eventType,
        Party party,
        Table? table = null)
    {
        Time = time;
        EventType = eventType;
        Party = party;
        Table = table;
    }
}