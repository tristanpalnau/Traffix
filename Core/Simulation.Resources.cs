using Traffix.Entities;
using Traffix.Events;

namespace Traffix.Core;

public partial class Simulation
{
    /// <summary>Returns the first available host, or <c>null</c> if all hosts are busy.</summary>
    private Host? FindAvailableHost()
    {
        return _hosts.FirstOrDefault(h => !h.IsBusy);
    }

    private Server? FindAvailableServer()
    {
        return _servers.FirstOrDefault(s => !s.IsBusy);
    }

    /// <summary>
    /// Returns the smallest unoccupied table that fits the party (best-fit),
    /// or <c>null</c> if no table is available.
    /// </summary>
    private Table? FindAvailableTableFor(Party party)
    {
        return _tables
            .Where(table => !table.IsOccupied && !_reservedTables.Contains(table) && table.Capacity >= party.Size)
            .OrderBy(table => table.Capacity)
            .FirstOrDefault();
    }

    /// <summary>
    /// Extracts the required table from an event, logging an error and returning
    /// <c>false</c> if the event has no associated table.
    /// </summary>
    private bool TryGetRequiredTable(SimulationEvent simEvent, out Table table)
    {
        if (simEvent.Table == null)
        {
            Console.WriteLine($"ERROR: {simEvent.EventType} event is missing a table at {_currentTime:F2}.");
            table = null!;
            return false;
        }

        table = simEvent.Table;
        return true;
    }

    /// <summary>
    /// Extracts the required host from an event, logging an error and returning
    /// <c>false</c> if the event has no associated host.
    /// </summary>
    private bool TryGetRequiredHost(SimulationEvent simEvent, out Host host)
    {
        if (simEvent.Host == null)
        {
            Console.WriteLine($"ERROR: {simEvent.EventType} event is missing a host at {_currentTime:F2}.");
            host = null!;
            return false;
        }

        host = simEvent.Host;
        return true;
    }

    private bool TryGetRequiredServer(SimulationEvent simEvent, out Server server)
    {
        if (simEvent.Server == null)
        {
            Console.WriteLine($"ERROR: {simEvent.EventType} event is missing a server at {_currentTime:F2}.");
            server = null!;
            return false;
        }

        server = simEvent.Server;
        return true;
    }

    /// <summary>
    /// Finds the first waiting party that has both a free table and a free host,
    /// then assigns the host and schedules a <see cref="EventType.HostAssigned"/> event.
    /// Unmatched parties remain in the queue in their original order.
    /// </summary>
    private void TrySeatNextWaitingParty()
    {
        if (_waitList.Count == 0) return;

        Host? host = FindAvailableHost();
        if (host == null) return;

        Queue<Party> tempQueue = new Queue<Party>();
        Party? selectedParty = null;
        Table? selectedTable = null;

        while (_waitList.Count > 0)
        {
            Party party = _waitList.Dequeue();

            if (selectedParty == null)
            {
                Table? table = FindAvailableTableFor(party);
                if (table != null)
                {
                    selectedParty = party;
                    selectedTable = table;
                    continue;
                }
            }

            tempQueue.Enqueue(party);
        }

        _waitList = tempQueue;

        if (selectedParty != null && selectedTable != null)
        {
            _reservedTables.Add(selectedTable);
            host.Assign(_currentTime);
            _eventQueue.AddEvent(new SimulationEvent(
                _currentTime,
                EventType.HostAssigned,
                selectedParty,
                selectedTable,
                host
            ));
        }
    }

    private void FlagServer(Party party, Table table)
    {
        _needsServer.Enqueue((party, table));
        TryAssignServerWork();
    }

    private void TryAssignServerWork()
    {
        while (TryTakeNextWaitingOrder() || TryServeNextWaitingParty())
        {
        }
    }

    private bool TryServeNextWaitingParty()
    {
        if (_needsServer.Count == 0) return false;

        Server? server = FindAvailableServer();
        if (server == null) return false;

        var (party, table) = _needsServer.Dequeue();
        server.Assign(_currentTime);
        _eventQueue.AddEvent(new SimulationEvent(
            _currentTime + _greetDelay,
            EventType.ServerGreet,
            party,
            table,
            server: server
        ));

        return true;
    }

    private bool TryTakeNextWaitingOrder()
    {
        if (_readyToOrder.Count == 0) return false;

        Server? server = FindAvailableServer();
        if (server == null) return false;

        var (party, table) = _readyToOrder.Dequeue();
        server.Assign(_currentTime);
        _eventQueue.AddEvent(new SimulationEvent(
            _currentTime + _takeOrderDelay,
            EventType.OrderPlaced,
            party,
            table,
            server: server
        ));

        return true;
    }
}
