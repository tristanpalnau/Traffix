namespace Traffix.Core;

public partial class Simulation
{
    private void PrintSimulationSummary()
    {
        Console.WriteLine("\n--- Table Utilization ---");

        foreach (var table in _tables)
        {
            double utilization = table.TotalOccupiedMinutes / _currentTime * 100;
            Console.WriteLine($"Table {table.Id}: {utilization:F2}% utilized");
        }

        Console.WriteLine("\n--- Host Utilization ---");

        foreach (var host in _hosts)
        {
            double utilization = host.TotalBusyMinutes / _currentTime * 100;
            Console.WriteLine($"Host {host.Id}: {utilization:F2}% utilized");
        }

        Console.WriteLine("\n--- Server Utilization ---");

        foreach (var server in _servers)
        {
            double utilization = server.TotalBusyMinutes / _currentTime * 100;
            Console.WriteLine($"Server {server.Id}: {utilization:F2}% utilized");
        }

        double averageWaitTime = _partiesSeated > 0
            ? _totalWaitTime / _partiesSeated
            : 0;

        Console.WriteLine($"\nAverage wait time: {averageWaitTime:0.00} minutes");
    }
}
