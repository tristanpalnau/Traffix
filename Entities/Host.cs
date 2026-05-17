namespace Traffix.Entities;

/// <summary>
/// A restaurant host who escorts parties to their tables.
/// Tracks busy/available state and accumulated busy time for utilization reporting.
/// </summary>
public class Host
{
    /// <summary>Unique identifier used in console output and metrics.</summary>
    public int Id { get; set; }

    /// <summary><c>true</c> while the host is escorting a party.</summary>
    public bool IsBusy { get; private set; }

    /// <summary>
    /// Running total of minutes the host has been busy across all seatings.
    /// Used to compute utilization percentage at simulation end.
    /// </summary>
    public double TotalBusyMinutes { get; private set; }

    private double? _busyStartTime;

    /// <summary>Marks the host as busy and records the start time.</summary>
    public void Assign(double currentTime)
    {
        IsBusy = true;
        _busyStartTime = currentTime;
    }

    /// <summary>
    /// Marks the host as available and accumulates the elapsed time into
    /// <see cref="TotalBusyMinutes"/>.
    /// </summary>
    public void Free(double currentTime)
    {
        IsBusy = false;

        if (_busyStartTime.HasValue)
        {
            TotalBusyMinutes += currentTime - _busyStartTime.Value;
            _busyStartTime = null;
        }
    }
}
