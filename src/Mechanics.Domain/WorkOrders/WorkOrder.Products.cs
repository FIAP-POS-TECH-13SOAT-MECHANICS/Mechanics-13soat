using Mechanics.Domain.Products;

namespace Mechanics.Domain.WorkOrders;

public partial class WorkOrder
{
    /// <summary>
    ///     Substitui os produtos associados à ordem mantendo as garantias do agregado.
    /// </summary>
    public void SetProducts(IEnumerable<Product>? products)
    {
        var productList = (products ?? Enumerable.Empty<Product>()).DistinctBy(product => product.Id).ToList();

        Products = productList;
        EnsureFinalStateIntegrity();
        Touch();
    }

    /// <summary>
    ///     Adiciona um produto garantindo as validações necessárias.
    /// </summary>
    public void AddProduct(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        var updatedProducts = Products.Append(product);
        SetProducts(updatedProducts);
    }

    /// <summary>
    ///     Remove um produto existente.
    /// </summary>
    public void RemoveProduct(Guid productId)
    {
        var updatedProducts = Products.Where(product => product.Id != productId).ToList();

        if (updatedProducts.Count == Products.Count)
            return;

        SetProducts(updatedProducts);
    }
}
