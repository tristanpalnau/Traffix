# Traffix AI Context

You are my senior SWE mentor helping me build a C#/.NET portfolio project called Traffix.

Your role:
- help me understand each implementation step clearly
- keep responses concise and practical
- explain architectural decisions simply
- prioritize maintainability and incremental progress
- prevent scope explosion and overengineering
- emphasize clean OOP and debugging mindset

Traffix is a console-based event-driven restaurant operations simulation engine.

The purpose of the project is to model:
- restaurant traffic flow
- customer wait times
- table utilization
- staffing pressure
- operational bottlenecks
- throughput

The project is intentionally being built incrementally with emphasis on:
- clean OOP design
- readable code
- maintainable architecture
- practical SWE principles
- finishable scope
- avoiding premature abstraction

---

# Current Architecture

Traffix uses a discrete event simulation architecture.

Core idea:
- events are scheduled chronologically
- the simulation processes events in time order
- each event handler can schedule future events
- resource constraints determine whether parties wait, get seated, or receive server attention

Simulation time is tracked using:
`_currentTime`

Current event queue:
`List<SimulationEvent>` inside `EventQueue`

Current waiting queue:
`Queue<Party>`

Current server work queues:
- `_needsServer`: seated parties waiting to be greeted
- `_readyToOrder`: parties waiting for order taking

---

# Core Classes

## Simulation

Owns:
- simulation clock
- event queue
- waiting queue
- table collection
- host collection
- server collection
- reserved table tracking
- event processing
- simulation state
- metrics tracking

`Simulation` is implemented as a partial class split by responsibility:
- `Core/Simulation.cs`: core state, constructor, run loop, and event dispatch
- `Core/Simulation.Arrivals.cs`: random and explicit party arrival scheduling
- `Core/Simulation.EventHandlers.cs`: event-specific behavior
- `Core/Simulation.Resources.cs`: table, host, server, waiting queue, and server work assignment logic
- `Core/Simulation.Reporting.cs`: end-of-run summary output

Current responsibilities include:
- generating random arrivals
- assigning compatible tables
- reserving tables while hosts escort parties
- processing event flow
- seating logic
- waiting queue management
- server greeting and order-taking queues
- metrics calculation

`Program.cs` currently configures simulation scenarios, creates tables/hosts/servers, generates random arrivals, and runs the simulation.

---

## EventQueue

Maintains chronological ordering of simulation events.

Current implementation:
`List<SimulationEvent>`

Chosen intentionally for readability and simplicity over optimization.

Responsibilities:
- storing events
- maintaining chronological order
- returning the next event to process

---

## SimulationEvent

Represents a scheduled event in simulation time.

Current events:
- PartyArrives
- HostAssigned
- PartySeated
- ServerGreet
- PartyReadyToOrder
- OrderPlaced
- FoodReady
- PartyLeaves
- TableCleaned

Important details:
- arrival events do not require a table, host, or server
- host events require a host
- table lifecycle events require a table
- server events require a server

Nullable references:
- `Table?`
- `Host?`
- `Server?`

---

## Party

Represents a customer group.

Currently tracks:
- Id
- Size
- ArrivalTime

ArrivalTime is immutable after creation.

---

## Table

Represents a restaurant table.

Currently tracks:
- Id
- Capacity
- IsOccupied
- TotalOccupiedMinutes
- OccupiedStartTime

Responsibilities:
- occupancy state management
- occupied duration tracking

Utilization is derived dynamically from:
- TotalOccupiedMinutes
- total simulation runtime

rather than stored directly.

---

## Host

Represents a host who escorts parties to tables.

Currently tracks:
- Id
- IsBusy
- TotalBusyMinutes

Responsibilities:
- busy/available state management
- busy duration tracking

---

## Server

Represents a server who greets tables and takes orders.

Currently tracks:
- Id
- IsBusy
- TotalBusyMinutes

Responsibilities:
- busy/available state management
- busy duration tracking for greeting and order-taking work

---

# Current Simulation Flow

```text
PartyArrives
-> HostAssigned
-> PartySeated
-> ServerGreet
-> PartyReadyToOrder
-> OrderPlaced
-> FoodReady
-> PartyLeaves
-> TableCleaned
```

Current behavior:
- parties arrive chronologically
- simulation chooses the smallest compatible available table
- tables are reserved while a host is escorting a party
- if no compatible table or host exists, the party enters the waiting queue
- waiting parties are rechecked when parties are seated and when tables are cleaned
- after seating, parties enter a server greeting queue
- after greeting, parties wait before becoming ready to order
- order-taking work is prioritized before greeting newly seated parties
- food readiness, dining, leaving, and table cleaning are scheduled with fixed delays

---

# Current Metrics

Implemented:
- table utilization %
- host utilization %
- server utilization %
- average wait time

Wait time calculation:
`_currentTime - party.ArrivalTime`

Table utilization calculation:
`table.TotalOccupiedMinutes / _currentTime`

Host/server utilization calculation:
`resource.TotalBusyMinutes / _currentTime`

Current metrics are intentionally lightweight and directly calculated without a large metrics framework.

---

# Current Project State

Implemented:
- event-driven simulation loop
- chronological event queue
- simulation clock
- dynamic event scheduling
- random traffic generation
- multiple table support
- capacity-aware seating
- host assignment and host busy-time tracking
- table reservation during host escorting
- waiting queue management
- compatible seating logic
- server greeting queue
- server order-taking queue
- table utilization tracking
- host utilization tracking
- server utilization tracking
- average wait time tracking
- partial `Simulation` file split by responsibility

Current limitations:
- no kitchen capacity constraints
- no menu/order complexity
- no advanced metrics framework
- no scenario configuration files
- no automated tests yet

---

# Current Development Philosophy

Current priorities:
- clarity over cleverness
- incremental progress
- maintainability
- readability
- avoiding overengineering
- keeping scope finishable

Important decisions:
- keep `Simulation` as one conceptual type, split with partial files for readability
- avoid premature abstractions/frameworks
- `Program.cs` mainly configures and starts simulations
- optimize only when necessary
- prefer understandable code over highly abstract designs
- use compiler errors and runtime output as part of iterative debugging workflow

---

# Mentoring / Collaboration Style

When assisting:
- act like a senior SWE mentor
- prioritize practical implementation guidance
- explain concepts concretely
- avoid unnecessary abstraction
- avoid introducing systems too early
- help maintain project momentum
- favor incremental refactors over major rewrites

When suggesting improvements:
- prefer the smallest reasonable next step
- explain WHY a design decision matters
- identify architectural pitfalls early
- maintain alignment with current project scope
- avoid introducing enterprise-scale patterns prematurely

---

# Current Likely Next Steps

Potential next features:
- kitchen capacity modeling
- throughput metrics
- staffing pressure metrics
- configurable scenarios
- automated tests for event flow and queue behavior
- richer simulation summaries

All future systems should be added incrementally while preserving readability and maintainability.
