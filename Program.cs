using Traffix.Config;
using Traffix.Core;
using Traffix.Entities;

var config = new SimulationConfig(
    Label: "Default Scenario",
    ServerCount: 1,
    HostCount: 1,
    TableLayout: [(2, 1), (4, 1), (6, 1)],
    PartyCount: 5,
    MeanInterArrivalMinutes: 5
);

EventQueue eventQueue = new EventQueue();

var tables = new List<Table>();
int tableId = 1;
foreach (var (capacity, count) in config.TableLayout)
{
    for (int i = 0; i < count; i++)
        tables.Add(new Table { Id = tableId++, Capacity = capacity, IsOccupied = false });
}

var hosts = new List<Host>();
for (int i = 1; i <= config.HostCount; i++)
    hosts.Add(new Host { Id = i });

var servers = new List<Server>();
for (int i = 1; i <= config.ServerCount; i++)
    servers.Add(new Server { Id = i });

Simulation simulation = new Simulation(eventQueue, tables, hosts, servers);

simulation.GenerateRandomArrivals(
    partyCount: config.PartyCount,
    meanInterArrivalMinutes: config.MeanInterArrivalMinutes,
    seed: config.Seed
);

simulation.Run(config.Label);
