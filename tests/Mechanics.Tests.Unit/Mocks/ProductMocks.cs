using Mechanics.Application.Products.Requests;
using Mechanics.Domain.Base;
using Mechanics.Domain.Products;

namespace Mechanics.Tests.Unit.Mocks;

public class ProductMocks
{
    public static CreateProductRequest BuildCreateRequest() => new()
    {
        Name = "Pneu",
        Description = "Pneu Pirelli",
        Type = ProductType.Part,
        Quantity = 10,
        Status = StatusType.ACTIVE
    };

    public static CreateProductRequest BuildInvalidCreateRequest() => new()
    {
        Name = "",
        Description = "",
        Type = ProductType.Part,
        Quantity = -1,
        Status = StatusType.ACTIVE
    };

    public static UpdateProductRequest BuildUpdateRequest() => new()
    {
        Name = "Roda",
        Description = "Roda prateada",
        Quantity = 10,
        Status = StatusType.ACTIVE
    };

    public static UpdateProductRequest BuildInvalidUpdateRequest() => new()
    {
        Name = "",
        Description = "",
        Quantity = -1,
        Status = StatusType.ACTIVE
    };

    public static Product CreateProduct(Guid id) => new()
    {
        Id = id,
        Name = "Roda",
        Description = "Roda preta",
        Type = ProductType.Part,
        Quantity = 10,
        Status = StatusType.ACTIVE
    };

    public static Product CreateInvalidProduct(Guid id) => new()
    {
        Id = id,
        Name = "",
        Description = "",
        Type = ProductType.Part,
        Quantity = -1,
        Status = StatusType.ACTIVE
    };
}
