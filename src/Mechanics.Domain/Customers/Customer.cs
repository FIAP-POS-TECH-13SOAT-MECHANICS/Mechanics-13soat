using Mechanics.Domain.Base;
using Mechanics.Domain.Vehicles;

namespace Mechanics.Domain.Customers;

public class Customer : AbstractEntity, INormalizable
{
    public required string FullName { get; set; }
    public required PersonalDocument Document { get; set; }
    public ICollection<Vehicle>? Vehicles { get; init; }

    public void Normalize()
    {
        FullName = FullName.ToUpper().Trim();
        Document.Normalize();
    }
}
