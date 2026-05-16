using Traffix.Core;
using Traffix.Entities;
using Traffix.Events;

EventQueue eventQueue = new EventQueue();

Simulation simulation = new Simulation(eventQueue);

Table table1 = new Table
{
    Id = 1,
    Capacity = 4,
    IsOccupied = false
};

Party party1 = new Party { Id = 1, Size = 4 };
Party party2 = new Party { Id = 2, Size = 2 };
Party party3 = new Party { Id = 3, Size = 3 };

simulation.AddEvent(new SimulationEvent(
    0,
    EventType.PartyArrives,
    party1,
    table1
));

simulation.AddEvent(new SimulationEvent(
    5,
    EventType.PartyArrives,
    party2,
    table1
));

simulation.AddEvent(new SimulationEvent(
    10,
    EventType.PartyArrives,
    party3,
    table1
));

simulation.Run();