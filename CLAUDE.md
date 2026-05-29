# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build
dotnet run
```

No tests exist yet. There is no linter configured beyond the compiler.

If `dotnet build` fails because files in `bin/Debug/net10.0` are locked by a running app, use a temporary output directory to verify compilation:

```bash
dotnet build -o .verify-build
```

Remove `.verify-build` after verification.

## Architecture

Traffix is a discrete event simulation engine for restaurant operations written in C# (.NET 10.0).

### Simulation Loop

`Simulation.Run()` dequeues events in chronological order, advances `_currentTime` to each event's timestamp, and processes them. Each handler may schedule new events, which are inserted into the queue in sorted order.

`Simulation` is implemented as a partial class split by responsibility:

- `Core/Simulation.cs`: core simulation state, constructor, run loop, and event dispatch
- `Core/Simulation.Arrivals.cs`: random arrival generation and explicit party arrival scheduling
- `Core/Simulation.EventHandlers.cs`: event-specific behavior
- `Core/Simulation.Resources.cs`: table, host, server, waiting queue, and server work assignment logic
- `Core/Simulation.Reporting.cs`: end-of-run metrics output

### Party Lifecycle

```text
PartyArrives -> HostAssigned -> PartySeated -> ServerGreet -> PartyReadyToOrder -> OrderPlaced -> FoodReady -> PartyLeaves -> TableCleaned
```

Fixed delays in minutes currently drive scheduling:

- `_seatDelay = 1`
- `_greetDelay = 1`
- `_readyToOrderDelay = 5`
- `_takeOrderDelay = 1`
- `_foodDelay = 10`
- `_diningDelay = 15`
- `_bussedDelay = 5`

### Seating Logic

- `FindAvailableTableFor(party)`: best-fit lookup, choosing the smallest unoccupied, unreserved table with enough capacity.
- `FindAvailableHost()`: returns the first non-busy host, or `null` if all hosts are occupied.
- A party is assigned to seating only when both a compatible table and a host are free.
- Tables are reserved while a host is escorting a party so another event cannot double-assign the same table.
- If a table or host is unavailable, the party joins the waiting queue.
- `TrySeatNextWaitingParty()`: scans the waiting queue for the first party that can be matched to a free table and host, preserving relative order for unmatched parties.

### Server Logic

- `FindAvailableServer()`: returns the first non-busy server, or `null` if all servers are occupied.
- After a party is seated, `FlagServer()` queues the party for a server greeting.
- `_needsServer` tracks parties waiting to be greeted.
- `_readyToOrder` tracks parties waiting for order taking.
- `TryAssignServerWork()` prioritizes order-taking before greeting new tables.
- Servers accumulate busy time for greeting and order-taking tasks.

### Key Files

| File | Role |
|------|------|
| `Core/Simulation.cs` | Simulation state, constructor, run loop, event dispatch |
| `Core/Simulation.Arrivals.cs` | Arrival generation and `SchedulePartyArrival()` |
| `Core/Simulation.EventHandlers.cs` | Handlers for each `EventType` |
| `Core/Simulation.Resources.cs` | Resource lookup, validation, seating, and server assignment |
| `Core/Simulation.Reporting.cs` | Table, host, server, and wait-time summary output |
| `Core/EventQueue.cs` | Sorted `List<SimulationEvent>` that re-sorts on each insert |
| `Events/EventType.cs` | Enum of the simulation event types |
| `Events/SimulationEvent.cs` | Event record: time, type, party, optional table, optional host, optional server |
| `Entities/Party.cs` | Party identity, size, and arrival time |
| `Entities/Table.cs` | Tracks occupancy and accumulates `TotalOccupiedMinutes` |
| `Entities/Host.cs` | Tracks host busy/available state and `TotalBusyMinutes` |
| `Entities/Server.cs` | Tracks server busy/available state and `TotalBusyMinutes` |
| `Program.cs` | Wires tables, hosts, servers, generates arrivals, and runs the simulation |

`Restaurant/` and `Metrics/` directories exist but are currently reserved for future work.

### Metrics (end-of-run)

- Table utilization: `TotalOccupiedMinutes / currentTime * 100` per table.
- Host utilization: `TotalBusyMinutes / currentTime * 100` per host.
- Server utilization: `TotalBusyMinutes / currentTime * 100` per server.
- Average wait time: arrival-to-seating across all seated parties.

## Development Notes

Keep changes incremental and readable. Prefer small, direct improvements over introducing abstractions before the simulation needs them.
