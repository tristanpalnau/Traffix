namespace Traffix.Events;

/// <summary>
/// The discrete steps in a party's lifecycle through the restaurant.
/// Events are processed in chronological order by <see cref="Traffix.Core.Simulation"/>.
/// </summary>
public enum EventType
{
    /// <summary>Party enters the restaurant and is either seated or added to the waiting queue.</summary>
    PartyArrives,
    /// <summary>A free host picks up a waiting party and begins escorting them to their table.</summary>
    HostAssigned,
    /// <summary>Party is assigned a table and occupancy tracking begins.</summary>
    PartySeated,
    /// <summary>Party submits their order; triggers the food preparation delay.</summary>
    OrderPlaced,
    /// <summary>Food is delivered; triggers the dining delay before the party leaves.</summary>
    FoodReady,
    /// <summary>Party vacates the table; triggers the bussing delay.</summary>
    PartyLeaves,
    /// <summary>Table is cleaned and made available; waiting parties are evaluated for seating.</summary>
    TableCleaned
}