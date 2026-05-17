namespace Traffix.Entities;

/// <summary>
/// A group of customers dining together. Immutable after creation.
/// </summary>
public class Party
{
    /// <summary>Unique identifier assigned at arrival generation time.</summary>
    public int Id { get; }

    /// <summary>Number of guests in the party; used to match against table capacity.</summary>
    public int Size { get; }

    /// <summary>
    /// Simulation time (minutes) when the party arrived. Used to calculate
    /// wait time once the party is seated.
    /// </summary>
    public double ArrivalTime { get; }

    public Party(int id, int size, double arrivalTime)
    {
        Id = id;
        Size = size;
        ArrivalTime = arrivalTime;
    }
}