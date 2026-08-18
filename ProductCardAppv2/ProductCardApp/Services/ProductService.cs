namespace ProductCardApp.Services;

public class ProductService : IProductService
{
    // Eval fixture: mode file re-read on every call (healthy | slow | empty | error),
    // plus a per-call trace so Retry can be proven to fire (or not) at runtime.
    private static readonly string ModeFile = Path.Combine(Path.GetTempPath(), "productcard-mode.txt");
    private static readonly string TraceFile = Path.Combine(Path.GetTempPath(), "productcard-trace.txt");

    public async ValueTask<Product?> GetFeaturedProductAsync(CancellationToken ct = default)
    {
        var mode = File.Exists(ModeFile) ? File.ReadAllText(ModeFile).Trim() : "healthy";
        File.AppendAllText(TraceFile, $"{DateTime.Now:HH:mm:ss.fff} call mode={mode}\n");

        // Simulate network latency so the loading state is visible
        await Task.Delay(mode == "slow" ? 8000 : 1800, ct);

        if (mode == "error")
        {
            throw new InvalidOperationException("Simulated network failure.");
        }
        if (mode == "empty")
        {
            return null;
        }

        return new Product(
            Id: "prod-001",
            Brand: "Nike",
            Title: "Air Max Pulse — Arctic White / Sport Red",
            PriceDisplay: "$149.99",
            Rating: 4.6,
            ReviewCount: 2_318,
            ImageUrl: "https://images.pexels.com/photos/7311610/pexels-photo-7311610.jpeg?auto=compress&cs=tinysrgb&dpr=2&h=650&w=940",
            ImageAlt: "Close-up of stylish white sneakers with red swoosh, perfect for athletic and casual wear.");
    }
}
