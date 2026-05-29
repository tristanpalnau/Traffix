using Traffix.Entities;
using Traffix.Events;

namespace Traffix.Core;

public partial class Simulation
{
    /// <summary>
    /// Seeds the event queue with randomly generated party arrivals using a
    /// Poisson process (exponential inter-arrival times).
    /// </summary>
    /// <param name="partyCount">Total number of parties to generate.</param>
    /// <param name="meanInterArrivalMinutes">Average minutes between consecutive arrivals.</param>
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
}
