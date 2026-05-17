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
PartyArrives → PartySeated → OrderPlaced → FoodReady → PartyLeaves → TableCleaned
```

Fixed delays (in minutes) drive scheduling: `_seatDelay=2`, `_orderDelay=6`, `_foodDelay=10`, `_diningDelay=15`, `_bussedDelay=5`.

### Seating Logic

- **`FindAvailableTableFor(party)`** — best-fit: smallest unoccupied table with sufficient capacity.
- **`FindWaitingPartyFor(table)`** — scans the FIFO waiting queue for the first party that fits the newly cleaned table; preserves relative order for unmatched parties.

### Key Files

| File | Role |
|------|------|
| `Core/Simulation.cs` | All event handlers, waiting queue, metrics, timing constants |
| `Core/EventQueue.cs` | Sorted `List<SimulationEvent>` — re-sorts on each insert |
| `Events/EventType.cs` | Enum of the six event types |
| `Events/SimulationEvent.cs` | Event record: time, type, party, optional table |
| `Entities/Party.cs` | Immutable: Id, Size, ArrivalTime |
| `Entities/Table.cs` | Tracks occupancy; accumulates `TotalOccupiedMinutes` |
| `Program.cs` | Wires tables and calls `SchedulePartyArrival()` to seed the run |

`Restaurant/` and `Metrics/` directories exist but are empty — reserved for future server and metrics work.

### Metrics (end-of-run)

- **Table utilization %**: `TotalOccupiedMinutes / currentTime * 100` per table.
- **Average wait time**: arrival-to-seating across all parties.
