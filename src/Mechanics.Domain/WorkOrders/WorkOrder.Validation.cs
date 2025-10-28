using Mechanics.Domain.Base.Validation;
namespace Mechanics.Domain.WorkOrders;

public partial class WorkOrder
{
    /// <inheritdoc />
    public void Validate(ValidationBuilder builder)
    {
        builder.AddValidation(CustomerId != Guid.Empty, nameof(CustomerId), "Customer is required.");
        builder.AddValidation(VehicleId != Guid.Empty, nameof(VehicleId), "Vehicle is required.");
        builder.AddValidation(!string.IsNullOrWhiteSpace(AccessKey), nameof(AccessKey), "Access key is required.");
        builder.AddConditionalValidation(!string.IsNullOrWhiteSpace(AccessKey), conditional =>
        {
            conditional.AddValidation(AccessKey!.Length == 8, nameof(AccessKey), "Access key must contain 8 digits.");
        });
        builder.AddValidation(LastUpdate >= CreationDate, nameof(LastUpdate),
            "The last update cannot be earlier than the creation date.");
        builder.AddConditionalValidation(Status is WorkOrderStatus.Completed or WorkOrderStatus.Delivered, conditional =>
        {
            conditional.AddValidation(HasItemsAssociated(), nameof(TotalAmount),
                "Completed work orders must contain at least one product or service.");
        });
    }
}
