namespace Traffix.Metrics;

public record SimulationResult(
    string ScenarioLabel,
    Dictionary<int, double> TableUtilization,
    Dictionary<int, double> HostUtilization,
    Dictionary<int, double> ServerUtilization,
    double AverageWaitTimeMinutes,
    int PartiesSeated,
    int PartiesWaited,
    double SimulationDurationMinutes
);
