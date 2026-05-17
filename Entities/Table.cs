namespace Traffix.Entities;

/// <summary>
/// A restaurant table that tracks occupancy state and accumulated usage time
/// for utilization reporting.
/// </summary>
public class Table
{
    /// <summary>Unique identifier used in console output and metrics.</summary>
    public int Id { get; set; }

    /// <summary>Maximum number of guests this table can seat.</summary>
    public int Capacity { get; set; }

    /// <summary><c>true</c> while a party is occupying the table.</summary>
    public bool IsOccupied { get; set; }

    /// <summary>
    /// Running total of minutes the table has been occupied across all seatings.
    /// Used to compute utilization percentage at simulation end.
    /// </summary>
    public double TotalOccupiedMinutes { get; private set; }

    /// <summary>
    /// Simulation time when the current party was seated, or <c>null</c> when unoccupied.
    /// </summary>
    public double? OccupiedStartTime { get; private set; }

    /// <summary>Marks the table as occupied and records the start time.</summary>
    public void Occupy(double currentTime)
    {
        IsOccupied = true;
        OccupiedStartTime = currentTime;
    }

    /// <summary>
    /// Marks the table as free and accumulates the elapsed occupancy into
    /// <see cref="TotalOccupiedMinutes"/>.
    /// </summary>
    public void Free(double currentTime)
    {
        IsOccupied = false;

        if (OccupiedStartTime.HasValue)
        {
            TotalOccupiedMinutes += currentTime - OccupiedStartTime.Value;
            OccupiedStartTime = null;
        }
    }
}