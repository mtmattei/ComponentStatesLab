namespace ProductCardApp.Services;

public partial record Product(
    string Id,
    string Brand,
    string Title,
    string PriceDisplay,
    double Rating,
    int ReviewCount,
    string ImageUrl,
    string ImageAlt);

public interface IProductService
{
    ValueTask<Product?> GetFeaturedProductAsync(CancellationToken ct = default);
}
