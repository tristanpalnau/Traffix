namespace Traffix.Config;

public record SimulationConfig(
    string Label,
    int ServerCount,
    int HostCount,
    List<(int Capacity, int Count)> TableLayout,
    int PartyCount,
    double MeanInterArrivalMinutes,
    int? Seed = null
);
