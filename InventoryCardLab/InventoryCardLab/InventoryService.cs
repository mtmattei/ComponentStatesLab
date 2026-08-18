namespace InventoryCardLab;

public interface IInventoryService
{
    ValueTask<IImmutableList<InventoryItem>> GetStockLevelsAsync(CancellationToken ct);
}

/// <summary>
/// Simulated warehouse inventory source. The mode file is re-read on every call so
/// each state (data / slow / empty / error) can be forced at runtime without a restart:
///   .states\mode.txt (beside the sln) containing one of: data | slow | empty | error
/// Every call appends a line to .states\trace.txt so a Retry can be proven to have
/// re-run the service.
/// </summary>
public sealed class InventoryService : IInventoryService
{
    private static readonly string FixtureDir = @"C:\Users\Platform006\ComponentStatesLab\InventoryCardLab\.states";
    private static readonly string ModeFile = Path.Combine(FixtureDir, "mode.txt");
    private static readonly string TraceFile = Path.Combine(FixtureDir, "trace.txt");

    private static readonly ImmutableList<InventoryItem> Items = ImmutableList.Create(
        new InventoryItem("WH-1042", "M8 Hex Bolts (box of 100)", 218, 50),
        new InventoryItem("WH-1107", "Nitrile Gloves L (pair)", 12, 40),
        new InventoryItem("WH-1213", "Stretch Wrap Roll 500mm", 64, 25),
        new InventoryItem("WH-1288", "Thermal Labels 4x6 (roll)", 7, 20),
        new InventoryItem("WH-1301", "Cardboard Boxes 40x30x30", 143, 60));

    public async ValueTask<IImmutableList<InventoryItem>> GetStockLevelsAsync(CancellationToken ct)
    {
        var mode = ReadMode();
        Trace(mode);

        await Task.Delay(mode == "slow" ? TimeSpan.FromSeconds(30) : TimeSpan.FromMilliseconds(800), ct);

        return mode switch
        {
            "empty" => ImmutableList<InventoryItem>.Empty,
            "error" => throw new InvalidOperationException("Inventory backend unreachable."),
            _ => Items,
        };
    }

    private static string ReadMode()
    {
        try
        {
            return File.Exists(ModeFile) ? File.ReadAllText(ModeFile).Trim().ToLowerInvariant() : "data";
        }
        catch
        {
            return "data";
        }
    }

    private static void Trace(string mode)
    {
        try
        {
            File.AppendAllText(TraceFile, $"{DateTimeOffset.Now:O} GetStockLevelsAsync mode={mode}{Environment.NewLine}");
        }
        catch
        {
            // Trace is diagnostics only; never fail the load over it.
        }
    }
}
