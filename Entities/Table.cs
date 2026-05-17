namespace Traffix.Entities;

public class Table
{
    public int Id { get; set; }
    public int Capacity { get; set; }
    public bool IsOccupied { get; set; }
    public double TotalOccupiedMinutes { get; private set; }
    public double? OccupiedStartTime { get; private set; }

    public void Occupy(double currentTime)
    {
        IsOccupied = true;
        OccupiedStartTime = currentTime;
    }

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