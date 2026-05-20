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

    /// <summary>
    /// The host handling this event, or <c>null</c> for events that do not
    /// involve a host (e.g. order, food, payment lifecycle events).
    /// </summary>
    public Host? Host { get; set; }

    /// <summary>
    /// The server handling this event, or <c>null</c> for events that do not
    /// involve a server (e.g. party arrives, party seated).
    /// </summary>
    public Server? Server { get; set; }

    public SimulationEvent(
        double time,
        EventType eventType,
        Party party,
        Table? table = null,
        Host? host = null,
        Server? server = null)
    {
        Time = time;
        EventType = eventType;
        Party = party;
        Table = table;
        Host = host;
        Server = server;
    }
}