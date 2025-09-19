using Mechanics.Domain.Base;
using Mechanics.Domain.Customers;

namespace Mechanics.Domain.Vehicles;

public class Vehicle : AbstractEntity
{
    public required string Brand { get; init; }
    public required string Model { get; init; }
    public required VehicleColor Color { get; init; }
    public required string Year { get; init; }
    public required Plate Plate { get; init; }
    public required Customer Owner { get; init; }
    public required Guid OwnerId { get; init; }
}
