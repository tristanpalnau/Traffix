using Traffix.Events;
using Traffix.Entities;

namespace Traffix.Core;

/// <summary>
/// The simulation engine. Owns the event queue, tables, and waiting queue,
/// and drives the main event-processing loop.
/// <para>
/// Typical usage: create a <see cref="Simulation"/>, seed arrivals with
/// <see cref="GenerateRandomArrivals"/> or <see cref="SchedulePartyArrival"/>,
/// then call <see cref="Run"/>.
/// </para>
/// </summary>
public class Simulation
{
    private readonly EventQueue _eventQueue;
    private readonly List<Table> _tables;
    private readonly List<Host> _hosts;
    private readonly List<Server> _servers;
    private readonly HashSet<Table> _reservedTables = new HashSet<Table>();
    private Queue<Party> _waitList = new Queue<Party>();
    private Queue<(Party party, Table table)> _needsServer = new Queue<(Party, Table)>();
    private Queue<(Party party, Table table)> _readyToOrder = new Queue<(Party, Table)>();


    private double _currentTime;
    private double _totalWaitTime = 0;
    private int _partiesSeated = 0;

    private int _seatDelay = 1;
    private int _greetDelay = 1;
    private int _readyToOrderDelay = 5;
    private int _takeOrderDelay = 1;
    private int _foodDelay = 10;
    private int _diningDelay = 15;
    private int _bussedDelay = 5;

    public Simulation(EventQueue eventQueue, List<Table> tables, List<Host> hosts, List<Server> servers)
    {
        _eventQueue = eventQueue;
        _tables = tables;
        _hosts = hosts;
        _servers = servers;
    }

    /// <summary>
    /// Processes all events in the queue until none remain, then prints
    /// the end-of-run summary.
    /// </summary>
    public void Run()
    {
        while (_eventQueue.HasEvents())
        {
            SimulationEvent currentEvent = _eventQueue.GetNextEvent();

            _currentTime = currentEvent.Time;

            ProcessEvent(currentEvent);
        }
        PrintSimulationSummary();
    }

    private void ProcessEvent(SimulationEvent simEvent)
    {
        switch (simEvent.EventType)
        {
            case EventType.PartyArrives:
                HandlePartyArrives(simEvent);
                break;

            case EventType.HostAssigned:
                HandleHostAssigned(simEvent);
                break;

            case EventType.PartySeated:
                HandlePartySeated(simEvent);
                break;

            case EventType.ServerGreet:
                HandleServerGreet(simEvent);
                break;

            case EventType.PartyReadyToOrder:
                HandlePartyReadyToOrder(simEvent);
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

    /// <summary>
    /// Seeds the event queue with randomly generated party arrivals using a
    /// Poisson process (exponential inter-arrival times).
    /// </summary>
    /// <param name="partyCount">Total number of parties to generate.</param>
    /// <param name="meanInterArrivalMinutes">Average minutes between consecutive arrivals (1/λ).</param>
    /// <param name="minPartySize">Minimum party size, inclusive. Defaults to 1.</param>
    /// <param name="maxPartySize">Maximum party size, inclusive. Defaults to 6.</param>
    /// <param name="seed">Optional RNG seed for reproducible runs.</param>
    public void GenerateRandomArrivals(
        int partyCount,
        double meanInterArrivalMinutes,
        int minPartySize = 1,
        int maxPartySize = 6,
        int? seed = null)
    {
        Random rng = seed.HasValue ? new Random(seed.Value) : new Random();
        double time = 0;

        for (int i = 1; i <= partyCount; i++)
        {
            time += -Math.Log(rng.NextDouble()) * meanInterArrivalMinutes;
            int size = rng.Next(minPartySize, maxPartySize + 1);
            SchedulePartyArrival(i, size, time);
        }
    }

    /// <summary>
    /// Schedules a single party arrival at an explicit simulation time.
    /// Prefer <see cref="GenerateRandomArrivals"/> for realistic traffic;
    /// use this for controlled test scenarios.
    /// </summary>
    public void SchedulePartyArrival(
        int partyId,
        int partySize,
        double arrivalTime)
    {
        Party party = new Party(
            partyId,
            partySize,
            arrivalTime);

        SimulationEvent arrivalEvent = new SimulationEvent(
            arrivalTime,
            EventType.PartyArrives,
            party,
            null);

        _eventQueue.AddEvent(arrivalEvent);
    }

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

        if (!TryGetRequiredTable(simEvent, out Table table))
            return;
        if (!TryGetRequiredServer(simEvent, out Server server))
            return;

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

        if (!TryGetRequiredTable(simEvent, out Table table))
            return;

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

        if (!TryGetRequiredTable(simEvent, out Table table))
            return;

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

        if (!TryGetRequiredTable(simEvent, out Table table))
            return;

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
        if (!TryGetRequiredTable(simEvent, out Table table))
            return;

        table.Free(_currentTime);

        Console.WriteLine($"Table {table.Id} cleaned at {_currentTime:F2}.");

        TrySeatNextWaitingParty();

        if (_waitList.Count > 0)
            Console.WriteLine($"Waiting parties remaining: {_waitList.Count}");
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

    private void PrintSimulationSummary()
    {
        Console.WriteLine("\n--- Table Utilization ---");

        foreach (var table in _tables)
        {
            double utilization = table.TotalOccupiedMinutes / _currentTime * 100;
            Console.WriteLine($"Table {table.Id}: {utilization:F2}% utilized");
        }

        Console.WriteLine("\n--- Host Utilization ---");

        foreach (var host in _hosts)
        {
            double utilization = host.TotalBusyMinutes / _currentTime * 100;
            Console.WriteLine($"Host {host.Id}: {utilization:F2}% utilized");
        }

        Console.WriteLine("\n--- Server Utilization ---");

        foreach (var server in _servers)
        {
            double utilization = server.TotalBusyMinutes / _currentTime * 100;
            Console.WriteLine($"Server {server.Id}: {utilization:F2}% utilized");
        }

        double averageWaitTime = _partiesSeated > 0
            ? _totalWaitTime / _partiesSeated
            : 0;

        Console.WriteLine($"\nAverage wait time: {averageWaitTime:0.00} minutes");
    }
}
