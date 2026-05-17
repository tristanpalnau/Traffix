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

simulation.GenerateRandomArrivals(partyCount: 20, meanInterArrivalMinutes: 5);

simulation.Run();