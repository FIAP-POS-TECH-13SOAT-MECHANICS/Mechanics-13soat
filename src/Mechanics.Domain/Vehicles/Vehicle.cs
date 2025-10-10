using Mechanics.Domain.Base;
using Mechanics.Domain.Customers;

namespace Mechanics.Domain.Vehicles;

public class Vehicle : AbstractEntity
{
    public required string Manufacturer { get; init; }
    public required string Model { get; init; }
    public required VehicleColor Color { get; init; }
    public required string Year { get; init; }
    public required LicensePlate LicensePlate { get; init; }
    public required string Chassis { get; init; }
    public required Customer Owner { get; init; }
    public required Guid OwnerId { get; init; }
}
