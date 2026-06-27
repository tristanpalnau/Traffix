using Traffix.Entities;
using Traffix.Events;

namespace Traffix.Core;

public partial class Simulation
{
    private void HandlePartyArrives(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        Console.WriteLine($"Party {party.Id} arrived at {_currentTime:F2}.");

        Table? availableTable = FindAvailableTableFor(party);
        Host? availableHost = FindAvailableHost();

        if (availableTable != null && availableHost != null)
        {
            _reservedTables.Add(availableTable);
            availableHost.Assign(_currentTime);
            _eventQueue.AddEvent(new SimulationEvent(
                _currentTime,
                EventType.HostAssigned,
                party,
                availableTable,
                availableHost
            ));
        }
        else
        {
            _waitList.Enqueue(party);
            _partiesWaited++;
            Console.WriteLine($"Party {party.Id} is waiting. Waiting count: {_waitList.Count}");
        }
    }

    private void HandleHostAssigned(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        if (!TryGetRequiredTable(simEvent, out Table table)) return;
        if (!TryGetRequiredHost(simEvent, out Host host)) return;

        Console.WriteLine($"Host {host.Id} escorting Party {party.Id} to Table {table.Id} at {_currentTime:F2}.");

        _eventQueue.AddEvent(new SimulationEvent(
            _currentTime + _seatDelay,
            EventType.PartySeated,
            party,
            table,
            host
        ));
    }

    private void HandlePartySeated(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        if (!TryGetRequiredTable(simEvent, out Table table)) return;
        if (!TryGetRequiredHost(simEvent, out Host host)) return;

        table.Occupy(_currentTime);
        _reservedTables.Remove(table);
        host.Free(_currentTime);

        double waitTime = _currentTime - party.ArrivalTime;
        _totalWaitTime += waitTime;
        _partiesSeated++;

        Console.WriteLine($"Party {party.Id} seated at Table {table.Id} at {_currentTime:F2}.");

        TrySeatNextWaitingParty();
        FlagServer(party, table);
    }

    private void HandleServerGreet(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        if (!TryGetRequiredTable(simEvent, out Table table)) return;
        if (!TryGetRequiredServer(simEvent, out Server server)) return;

        server.Free(_currentTime);

        Console.WriteLine($"Server {server.Id} greeted Party {party.Id} at Table {table.Id} at {_currentTime:F2}.");

        _eventQueue.AddEvent(new SimulationEvent(
            _currentTime + _readyToOrderDelay,
            EventType.PartyReadyToOrder,
            party,
            table
        ));

        TryAssignServerWork();
    }

    private void HandlePartyReadyToOrder(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        if (!TryGetRequiredTable(simEvent, out Table table)) return;

        _readyToOrder.Enqueue((party, table));
        Console.WriteLine($"Party {party.Id} is ready to order at {_currentTime:F2}.");

        TryAssignServerWork();
    }

    private void HandleOrderPlaced(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        if (!TryGetRequiredTable(simEvent, out Table table)) return;
        if (!TryGetRequiredServer(simEvent, out Server server)) return;

        server.Free(_currentTime);

        Console.WriteLine($"Party {party.Id} order placed at {_currentTime:F2}.");

        TryAssignServerWork();

        _eventQueue.AddEvent(new SimulationEvent(
            _currentTime + _foodDelay,
            EventType.FoodReady,
            party,
            table
        ));
    }

    private void HandleFoodReady(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        if (!TryGetRequiredTable(simEvent, out Table table)) return;

        Console.WriteLine($"Party {party.Id}'s food ready at {_currentTime:F2}.");

        _eventQueue.AddEvent(new SimulationEvent(
            _currentTime + _diningDelay,
            EventType.PartyLeaves,
            party,
            table
        ));
    }

    private void HandlePartyLeaves(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        if (!TryGetRequiredTable(simEvent, out Table table)) return;

        Console.WriteLine($"Party {party.Id} left at {_currentTime:F2}.");

        _eventQueue.AddEvent(new SimulationEvent(
            _currentTime + _bussedDelay,
            EventType.TableCleaned,
            party,
            table
        ));
    }

    private void HandleTableCleaned(SimulationEvent simEvent)
    {
        if (!TryGetRequiredTable(simEvent, out Table table)) return;

        table.Free(_currentTime);

        Console.WriteLine($"Table {table.Id} cleaned at {_currentTime:F2}.");

        TrySeatNextWaitingParty();

        if (_waitList.Count > 0)
        {
            Console.WriteLine($"Waiting parties remaining: {_waitList.Count}");
        }
    }
}
