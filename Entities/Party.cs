namespace Traffix.Entities;

public class Party
{
    public int Id { get; }
    public int Size { get; }
    public double ArrivalTime { get; }

    public Party(int id, int size, double arrivalTime)
    {
        Id = id;
        Size = size;
        ArrivalTime = arrivalTime;
    }
}