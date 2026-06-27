using Traffix.Metrics;

namespace Traffix.Core;

public partial class Simulation
{
    internal SimulationResult BuildResult(string label)
    {
        return new SimulationResult(
            label,
            _tables.ToDictionary(t => t.Id, t => t.TotalOccupiedMinutes / _currentTime * 100),
            _hosts.ToDictionary(h => h.Id, h => h.TotalBusyMinutes / _currentTime * 100),
            _servers.ToDictionary(s => s.Id, s => s.TotalBusyMinutes / _currentTime * 100),
            _partiesSeated > 0 ? _totalWaitTime / _partiesSeated : 0,
            _partiesSeated,
            _partiesWaited,
            _currentTime
        );
    }

    private void PrintSimulationSummary(string label)
    {
        SimulationResult result = BuildResult(label);

        Console.WriteLine("\n--- Table Utilization ---");
        foreach (var kvp in result.TableUtilization)
            Console.WriteLine($"Table {kvp.Key}: {kvp.Value:F2}% utilized");

        Console.WriteLine("\n--- Host Utilization ---");
        foreach (var kvp in result.HostUtilization)
            Console.WriteLine($"Host {kvp.Key}: {kvp.Value:F2}% utilized");

        Console.WriteLine("\n--- Server Utilization ---");
        foreach (var kvp in result.ServerUtilization)
            Console.WriteLine($"Server {kvp.Key}: {kvp.Value:F2}% utilized");

        Console.WriteLine($"\nAverage wait time: {result.AverageWaitTimeMinutes:0.00} minutes");
    }
}
