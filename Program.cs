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

List<Host> hosts = new List<Host>
{
    new Host { Id = 1 }
};

List<Server> servers = new List<Server>
{
    new Server { Id = 1 }
};

Simulation simulation = new Simulation(eventQueue, tables, hosts, servers);

simulation.GenerateRandomArrivals(partyCount: 5, meanInterArrivalMinutes: 5);

simulation.Run();