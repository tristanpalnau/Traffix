# Traffix

Traffix is an event-driven restaurant operations simulation engine built in C#.

The goal of the project is to model restaurant traffic flow, table turnover, guest wait times, and operational bottlenecks using a discrete event simulation approach.

## Current Features

- Event queue that processes events in chronological order
- Simulation clock based on event timestamps
- Party arrival, seating, ordering, food ready, leaving, and table cleaning events
- Waiting queue for parties when a table is occupied
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
↓
Party Seated
↓
Order Placed
↓
Food Ready
↓
Party Leaves
↓
Table Cleaned
```

If the table is occupied, arriving parties are added to a waiting queue and seated later when the table becomes available.

## Project Goals

Future improvements may include:

- Multiple tables
- Multiple servers
- Kitchen capacity modeling
- Table utilization metrics
- Average wait time calculations
- Randomized arrival patterns
- Staffing scenario comparison
- Bottleneck analysis

## Why I Built This

I built Traffix to practice object-oriented design, event-driven architecture, data structures, and simulation modeling in a realistic business context.