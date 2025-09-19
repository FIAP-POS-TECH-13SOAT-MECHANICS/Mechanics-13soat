using Mechanics.Domain.Base;
using Mechanics.Domain.Vehicles;

namespace Mechanics.Domain.Customers;

public class Customer : AbstractEntity
{
    public required string FullName { get; init; }
    public required PersonalDocument Document { get; init; }
    public ICollection<Vehicle>? Vehicles { get; init; }
}
