namespace Traffix.Entities;

/// <summary>
/// A restaurant server who assists parties at their tables.
/// Tracks busy/available state and accumulated busy time for utilization reporting.
/// </summary>
public class Server
{
    /// <summary>Unique identifier used in console output and metrics.</summary>
    public int Id { get; set; }

    /// <summary><c>true</c> while the server is completing a task for a party.</summary>
    public bool IsBusy { get; private set; }

    /// <summary>
    /// Running total of minutes the server has been busy across all assigned tasks.
    /// Used to compute utilization percentage at simulation end.
    /// </summary>
    public double TotalBusyMinutes { get; private set; }

    private double? _busyStartTime;

    /// <summary>Marks the server as busy and records the start time.</summary>
    public void Assign(double currentTime)
    {
        IsBusy = true;
        _busyStartTime = currentTime;
    }

    /// <summary>
    /// Marks the server as available and accumulates the elapsed time into
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
