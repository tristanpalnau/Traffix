# Traffix

Traffix is an event-driven restaurant operations simulation engine built in C#.

The goal of the project is to model restaurant traffic flow, table turnover, guest wait times, and operational bottlenecks using a discrete event simulation approach.

## Current Features

- Event queue that processes events in chronological order
- Simulation clock based on event timestamps
- Random party arrival generation using exponential inter-arrival times
- Party arrival, host assignment, seating, server greeting, ordering, food ready, leaving, and table cleaning events
- Waiting queue for parties when no suitable table or host is available
- Table reservation while a host is escorting a party, preventing double assignment
- Server work queues for greeting parties and taking orders
- Separate server busy time for greeting and order-taking work
- Table, host, and server utilization reporting
- Average guest wait time reporting
- Modular event handlers for cleaner simulation logic
- Console-based output for early testing

## Technologies

- C#
- .NET
- Object-oriented programming
- Queue-based event scheduling
- Discrete event simulation

## Current Simulation Flow

```text
Party Arrives
|
v
Host Assigned
|
v
Party Seated
|
v
Server Greet
|
v
Party Ready to Order
|
v
Order Placed
|
v
Food Ready
|
v
Party Leaves
|
v
Table Cleaned
```

If no suitable table or host is available, the arriving party is added to a waiting queue. When a table is cleaned or a host becomes available, the simulation tries to seat the next waiting party that can fit at an open table.

After a party is seated, they enter the server workflow. A server is busy while greeting a table, then becomes available again while the party decides what to order. When the party is ready to order, the simulation assigns an available server to take the order.

## Running Locally

From the project root:

```bash
dotnet run
```

The sample setup in `Program.cs` creates tables, hosts, servers, generates random arrivals, and runs the simulation to completion.

## Project Structure

```text
Core/
  EventQueue.cs
  Simulation.cs                  core simulation state and run loop
  Simulation.Arrivals.cs         party arrival scheduling
  Simulation.EventHandlers.cs    event-specific simulation behavior
  Simulation.Resources.cs        table, host, server, and queue assignment logic
  Simulation.Reporting.cs        end-of-run summary output
Entities/
  Host.cs
  Party.cs
  Server.cs
  Table.cs
Events/
  EventType.cs
  SimulationEvent.cs
```

## Project Goals

Future improvements may include:

- Multiple servers
- Kitchen capacity modeling
- Separate table occupied time and table unavailable time metrics
- Randomized arrival patterns
- Staffing scenario comparison
- Bottleneck analysis

## Why I Built This

I built Traffix to practice object-oriented design, event-driven architecture, data structures, and simulation modeling in a realistic business context.
