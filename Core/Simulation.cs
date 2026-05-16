using Traffix.Events;
using Traffix.Entities;

namespace Traffix.Core;

public class Simulation
{
    private readonly EventQueue _eventQueue;
    private readonly Queue<Party> _waitingParties = new Queue<Party>();

    private int _currentTime;

    private int _seatDelay = 2;
    private int _orderDelay = 6;
    private int _foodDelay = 10;
    private int _diningDelay = 15;
    private int _bussedDelay = 5;

    public Simulation(EventQueue eventQueue)
    {
        _eventQueue = eventQueue;
    }

    public void Run()
    {
        while (_eventQueue.HasEvents())
        {
            SimulationEvent currentEvent = _eventQueue.GetNextEvent();

            _currentTime = currentEvent.Time;

            ProcessEvent(currentEvent);
        }
    }

    public void AddEvent(SimulationEvent newEvent)
    {
        _eventQueue.AddEvent(newEvent);
    }

    private void ProcessEvent(SimulationEvent simEvent)
    {
        switch (simEvent.Type)
        {
            case EventType.PartyArrives:
                HandlePartyArrives(simEvent);
                break;

            case EventType.PartySeated:
                HandlePartySeated(simEvent);
                break;

            case EventType.OrderPlaced:
                HandleOrderPlaced(simEvent);
                break;

            case EventType.FoodReady:
                HandleFoodReady(simEvent);
                break;

            case EventType.PartyLeaves:
                HandlePartyLeaves(simEvent);
                break;

            case EventType.TableCleaned:
                HandleTableCleaned(simEvent);
                break;
        }
    }

    private void HandlePartyArrives(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;
        Table table = simEvent.Table;

        Console.WriteLine($"Party {party.Id} arrived at {_currentTime}.");

        if (!table.IsOccupied)
        {
            AddEvent(new SimulationEvent(
                _currentTime + _seatDelay,
                EventType.PartySeated,
                party,
                table
            ));
        }
        else
        {
            _waitingParties.Enqueue(party);
            Console.WriteLine($"Party {party.Id} is waiting. Waiting count: {_waitingParties.Count}");
        }
    }

    private void HandlePartySeated(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;
        Table table = simEvent.Table;

        table.IsOccupied = true;

        Console.WriteLine($"Party {party.Id} seated at Table {table.Id} at {_currentTime}.");

        AddEvent(new SimulationEvent(
            _currentTime + _orderDelay,
            EventType.OrderPlaced,
            party,
            table
        ));
    }

    private void HandleOrderPlaced(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;
        Table table = simEvent.Table;

        Console.WriteLine($"Party {party.Id} order placed at {_currentTime}.");

        AddEvent(new SimulationEvent(
            _currentTime + _foodDelay,
            EventType.FoodReady,
            party,
            table
        ));
    }

    private void HandleFoodReady(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;
        Table table = simEvent.Table;

        Console.WriteLine($"Party {party.Id}'s food ready at {_currentTime}.");

        AddEvent(new SimulationEvent(
            _currentTime + _diningDelay,
            EventType.PartyLeaves,
            party,
            table
        ));
    }

    private void HandlePartyLeaves(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;
        Table table = simEvent.Table;

        Console.WriteLine($"Party {party.Id} left at {_currentTime}.");

        AddEvent(new SimulationEvent(
            _currentTime + _bussedDelay,
            EventType.TableCleaned,
            party,
            table
        ));
    }

    private void HandleTableCleaned(SimulationEvent simEvent)
    {
        Table table = simEvent.Table;

        table.IsOccupied = false;

        Console.WriteLine($"Table {table.Id} cleaned at {_currentTime}.");

        if (_waitingParties.Count > 0)
        {
            Party nextParty = _waitingParties.Dequeue();

            Console.WriteLine($"Waiting parties remaining: {_waitingParties.Count}");

            AddEvent(new SimulationEvent(
                _currentTime + _seatDelay,
                EventType.PartySeated,
                nextParty,
                table
            ));
        }
    }
}