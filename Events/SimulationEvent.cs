using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Traffix.Entities;

namespace Traffix.Events;

public class SimulationEvent
{
    public double Time { get; set; }
    public EventType EventType { get; set; }
    public Party Party { get; set; }
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