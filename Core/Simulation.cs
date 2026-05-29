using Traffix.Entities;
using Traffix.Events;

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
public partial class Simulation
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
}
