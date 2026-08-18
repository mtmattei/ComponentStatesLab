using FluentStatesProbe.Models;

namespace FluentStatesProbe.Services;

/// <summary>
/// Simulated order source. A runtime-rewritable mode file is re-read on every
/// call so each UI state can be forced while the app is running:
///   %TEMP%\FluentStatesProbe.mode  containing one of: normal | slow | empty | error
/// Every call appends a line to %TEMP%\FluentStatesProbe.trace.log so a retry
/// can be proven to have re-run the service.
/// </summary>
public sealed class OrderService
{
    private static readonly string ModeFilePath = Path.Combine(Path.GetTempPath(), "FluentStatesProbe.mode");
    private static readonly string TraceFilePath = Path.Combine(Path.GetTempPath(), "FluentStatesProbe.trace.log");

    public async Task<IReadOnlyList<Order>> GetRecentOrdersAsync(CancellationToken ct = default)
    {
        var mode = ReadMode();
        AppendTrace(mode);

        // Simulated network latency
        await Task.Delay(mode == "slow" ? 10000 : 1200, ct);

        return mode switch
        {
            "empty" => Array.Empty<Order>(),
            "error" => throw new InvalidOperationException("Order service unavailable."),
            _ => new List<Order>
            {
                new("ORD-1042", "Camille Roy", "$1,284.00", "Shipped", "Aug 15"),
                new("ORD-1041", "Devon Price", "$312.50", "Processing", "Aug 15"),
                new("ORD-1039", "Ana Lucia Torres", "$98.75", "Shipped", "Aug 14"),
                new("ORD-1036", "Marc-Andre Gagnon", "$2,410.00", "Delivered", "Aug 13"),
                new("ORD-1033", "Priya Natarajan", "$76.20", "Delivered", "Aug 12"),
            },
        };
    }

    private static string ReadMode()
    {
        try
        {
            return File.Exists(ModeFilePath)
                ? File.ReadAllText(ModeFilePath).Trim().ToLowerInvariant()
                : "normal";
        }
        catch
        {
            return "normal";
        }
    }

    private static void AppendTrace(string mode)
    {
        try
        {
            File.AppendAllText(TraceFilePath, $"{DateTime.Now:O} GetRecentOrdersAsync mode={mode}{Environment.NewLine}");
        }
        catch
        {
            // Trace is a test fixture; never let it break the data path.
        }
    }
}
