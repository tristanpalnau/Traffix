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
    private Queue<Party> _waitingParties = new Queue<Party>();


    private double _currentTime;
    private double _totalWaitTime = 0;
    private int _partiesSeated = 0;

    private int _seatDelay = 2;
    private int _orderDelay = 6;
    private int _foodDelay = 10;
    private int _diningDelay = 15;
    private int _bussedDelay = 5;

    public Simulation(EventQueue eventQueue, List<Table> tables)
    {
        _eventQueue = eventQueue;
        _tables = tables;
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

    /// <summary>
    /// Returns the smallest unoccupied table that fits the party (best-fit),
    /// or <c>null</c> if no table is available.
    /// </summary>
    private Table? FindAvailableTableFor(Party party)
    {
        return _tables
            .Where(table => !table.IsOccupied && table.Capacity >= party.Size)
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
            Console.WriteLine($"ERROR: {simEvent.EventType} event is missing a table at {_currentTime}.");
            table = null!;
            return false;
        }

        table = simEvent.Table;
        return true;
    }

    private void HandlePartyArrives(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        Console.WriteLine($"Party {party.Id} arrived at {_currentTime}.");

        Table? availableTable = FindAvailableTableFor(party);

        if (availableTable != null)
        {
            _eventQueue.AddEvent(new SimulationEvent(
                _currentTime + _seatDelay,
                EventType.PartySeated,
                party,
                availableTable
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

        if (!TryGetRequiredTable(simEvent, out Table table))
            return;

        table.Occupy(_currentTime);

        double waitTime = _currentTime - party.ArrivalTime;
        _totalWaitTime += waitTime;
        _partiesSeated++;

        Console.WriteLine($"Party {party.Id} seated at Table {table.Id} at {_currentTime}.");

        _eventQueue.AddEvent(new SimulationEvent(
            _currentTime + _orderDelay,
            EventType.OrderPlaced,
            party,
            table
        ));
    }

    private void HandleOrderPlaced(SimulationEvent simEvent)
    {
        Party party = simEvent.Party;

        if (!TryGetRequiredTable(simEvent, out Table table))
            return;

        Console.WriteLine($"Party {party.Id} order placed at {_currentTime}.");

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

        Console.WriteLine($"Party {party.Id}'s food ready at {_currentTime}.");

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

        Console.WriteLine($"Party {party.Id} left at {_currentTime}.");

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

        Console.WriteLine($"Table {table.Id} cleaned at {_currentTime}.");

        if (_waitingParties.Count > 0)
        {
            Party? nextParty = FindWaitingPartyFor(table);

            if (nextParty != null)
            {
                _eventQueue.AddEvent(new SimulationEvent(
                    _currentTime + _seatDelay,
                    EventType.PartySeated,
                    nextParty,
                    table
                ));
            }

            Console.WriteLine($"Waiting parties remaining: {_waitingParties.Count}");
        }
    }

    /// <summary>
    /// Scans the waiting queue for the first party that fits <paramref name="table"/>.
    /// Unmatched parties are re-queued in their original order.
    /// </summary>
    private Party? FindWaitingPartyFor(Table table)
    {
        Queue<Party> tempQueue = new Queue<Party>();

        Party? selectedParty = null;

        while (_waitingParties.Count > 0)
        {
            Party party = _waitingParties.Dequeue();

            if (selectedParty == null && party.Size <= table.Capacity)
            {
                selectedParty = party;
            }
            else
            {
                tempQueue.Enqueue(party);
            }
        }

        _waitingParties = tempQueue;

        return selectedParty;
    }

    private void PrintSimulationSummary()
    {
        Console.WriteLine("\n--- Table Utilization ---");

        foreach (var table in _tables)
        {
            double utilization =
                table.TotalOccupiedMinutes / _currentTime * 100;

            Console.WriteLine(
                $"Table {table.Id}: {utilization:F2}% utilized");
        }

        double averageWaitTime = _partiesSeated > 0
            ? _totalWaitTime / _partiesSeated
            : 0;

        Console.WriteLine($"Average wait time: {averageWaitTime:0.00} minutes");
    }
}