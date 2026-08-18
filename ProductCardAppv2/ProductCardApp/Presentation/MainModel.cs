using ProductCardApp.Services;

namespace ProductCardApp.Presentation;

public partial record MainModel(IProductService ProductService)
{
    public IFeed<Product> Product => Feed.Async(async ct =>
    {
        var result = await ProductService.GetFeaturedProductAsync(ct);
        return result ?? throw new InvalidOperationException("No product available.");
    });

    public async ValueTask AddToCart(CancellationToken ct)
    {
        await Task.CompletedTask;
    }
}
