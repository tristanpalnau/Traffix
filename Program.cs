using Traffix.Core;
using Traffix.Entities;
using Traffix.Events;

EventQueue eventQueue = new EventQueue();

List<Table> tables = new List<Table>
{
    new Table { Id = 1, Capacity = 2, IsOccupied = false },
    new Table { Id = 2, Capacity = 4, IsOccupied = false },
    new Table { Id = 3, Capacity = 6, IsOccupied = false }
};

Simulation simulation = new Simulation(eventQueue, tables);

simulation.SchedulePartyArrival(1, 2, 0);
simulation.SchedulePartyArrival(2, 5, 6);
simulation.SchedulePartyArrival(3, 5, 12);

simulation.Run();