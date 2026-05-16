using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Traffix.Entities;

namespace Traffix.Events;

public class SimulationEvent
{
    public int Time { get; set; }
    public EventType Type { get; set; }
    public Party Party { get; set; }
    public Table Table { get; set; }

    public SimulationEvent(int time, EventType type, Party party, Table table)
    {
        Time = time;
        Type = type;
        Party = party;
        Table = table;
    }
}