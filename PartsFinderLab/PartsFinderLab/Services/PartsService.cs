using PartsFinderLab.Models;

namespace PartsFinderLab.Services;

public sealed class PartsService
{
    private static readonly Part[] Catalog =
    {
        new("BRK-2041", "Brake caliper, front left", "A-12", 8),
        new("BRK-2042", "Brake caliper, front right", "A-12", 6),
        new("FLT-0918", "Cabin air filter", "C-04", 41),
        new("BLT-3307", "Serpentine belt, 6-rib", "B-21", 17),
        new("SNS-5512", "Wheel speed sensor", "D-08", 3),
    };

    public async Task<WarehouseInfo> GetWarehouseAsync(CancellationToken ct = default)
    {
        var modes = ServiceFixture.ReadModes();
        ServiceFixture.Trace("GetWarehouse", modes);

        await Task.Delay(900, ct);

        if (modes.Contains("warehouse-slow"))
        {
            await Task.Delay(ServiceFixture.SlowExtraDelayMs(modes), ct);
        }

        if (modes.Contains("warehouse-error"))
        {
            throw new InvalidOperationException("Depot service unavailable (HTTP 503).");
        }

        return new WarehouseInfo("Lachine Depot", "Montreal, QC", "18:42");
    }

    public async Task<IReadOnlyList<Part>> SearchPartsAsync(string query, CancellationToken ct = default)
    {
        var modes = ServiceFixture.ReadModes();
        ServiceFixture.Trace($"SearchParts q='{query}'", modes);

        await Task.Delay(1100, ct);

        if (modes.Contains("results-slow"))
        {
            await Task.Delay(ServiceFixture.SlowExtraDelayMs(modes), ct);
        }

        if (modes.Contains("results-error"))
        {
            throw new InvalidOperationException("Catalogue service unavailable (HTTP 503).");
        }

        if (modes.Contains("results-empty"))
        {
            // Absence is data, never an exception: an empty result is a successful call.
            return Array.Empty<Part>();
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return Catalog;
        }

        return Catalog
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || p.Sku.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}

/// <summary>
/// Test fixture hook so the loading / empty / error states are reachable at runtime.
/// Both files live in %TEMP% and are re-read on every service call, so the mode can be
/// flipped while the app is running (which is what makes Retry verifiable).
///   %TEMP%\partsfinderlab.mode   whitespace/comma separated tokens:
///                                warehouse-slow, warehouse-error,
///                                results-slow, results-empty, results-error,
///                                slow-&lt;ms&gt; (overrides the *-slow extra latency)
///  %TEMP%\partsfinderlab.trace  one appended line per service call.
/// No file (or an empty file) means normal behaviour.
/// </summary>
internal static class ServiceFixture
{
    private const int DefaultSlowExtraDelayMs = 8000;

    /// <summary>
    /// Extra latency added by the *-slow tokens, long enough to inspect the skeleton.
    /// Override with a "slow-&lt;ms&gt;" token in the mode file.
    /// </summary>
    public static int SlowExtraDelayMs(HashSet<string> modes)
    {
        foreach (var mode in modes)
        {
            if (mode.StartsWith("slow-", StringComparison.Ordinal)
                && int.TryParse(mode.AsSpan(5), out var ms)
                && ms is > 0 and <= 120000)
            {
                return ms;
            }
        }

        return DefaultSlowExtraDelayMs;
    }

    private static readonly string ModePath =
        Path.Combine(Path.GetTempPath(), "partsfinderlab.mode");

    private static readonly string TracePath =
        Path.Combine(Path.GetTempPath(), "partsfinderlab.trace");

    public static HashSet<string> ReadModes()
    {
        try
        {
            if (!File.Exists(ModePath))
            {
                return new HashSet<string>();
            }

            return File.ReadAllText(ModePath)
                .Split(new[] { ' ', ',', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLowerInvariant())
                .ToHashSet();
        }
        catch
        {
            return new HashSet<string>();
        }
    }

    public static void Trace(string call, HashSet<string> modes)
    {
        try
        {
            File.AppendAllText(
                TracePath,
                $"{DateTimeOffset.UtcNow:O}\t{call}\tmodes=[{string.Join(",", modes)}]{Environment.NewLine}");
        }
        catch
        {
            // Tracing must never affect the app.
        }
    }
}
