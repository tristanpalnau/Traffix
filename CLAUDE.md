# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
dotnet build
dotnet run
```

No tests exist yet. There is no linter configured beyond the compiler.

## Architecture

Traffix is a discrete event simulation engine for restaurant operations written in C# (.NET 10.0).

### Simulation Loop

`Simulation.Run()` dequeues events in chronological order, advances `_currentTime` to each event's timestamp, and processes them. Each handler may schedule new events, which are inserted into the queue in sorted order.

### Party Lifecycle

```
PartyArrives → HostAssigned → PartySeated → OrderPlaced → FoodReady → PartyLeaves → TableCleaned
```

Fixed delays (in minutes) drive scheduling: `_seatDelay=2`, `_orderDelay=6`, `_foodDelay=10`, `_diningDelay=15`, `_bussedDelay=5`.

### Seating Logic

- **`FindAvailableTableFor(party)`** — best-fit: smallest unoccupied table with sufficient capacity.
- **`FindAvailableHost()`** — returns the first non-busy host, or `null` if all hosts are occupied.
- A party is seated only when both a table **and** a host are free. If either is unavailable, the party joins the FIFO waiting queue.
- **`TrySeatNextWaitingParty()`** — called after `PartySeated` and `TableCleaned`; scans the waiting queue for the first party that can be matched to a free table and host; preserves relative order for unmatched parties.

### Key Files

| File | Role |
|------|------|
| `Core/Simulation.cs` | All event handlers, waiting queue, metrics, timing constants |
| `Core/EventQueue.cs` | Sorted `List<SimulationEvent>` — re-sorts on each insert |
| `Events/EventType.cs` | Enum of the seven event types |
| `Events/SimulationEvent.cs` | Event record: time, type, party, optional table, optional host |
| `Entities/Party.cs` | Immutable: Id, Size, ArrivalTime |
| `Entities/Table.cs` | Tracks occupancy; accumulates `TotalOccupiedMinutes` |
| `Entities/Host.cs` | Tracks busy/available state; accumulates `TotalBusyMinutes` |
| `Program.cs` | Wires tables and hosts, calls `GenerateRandomArrivals()` to seed the run |

`Restaurant/` and `Metrics/` directories exist but are empty — reserved for future server and metrics work.

### Metrics (end-of-run)

- **Table utilization %**: `TotalOccupiedMinutes / currentTime * 100` per table.
- **Host utilization %**: `TotalBusyMinutes / currentTime * 100` per host.
- **Average wait time**: arrival-to-seating across all parties.
