using ComponentStatesLab.Business;

namespace ComponentStatesLab.Services;

/// <summary>
/// In-memory portfolio data for the lab.
///
/// The LAB_MODE environment variable drives the service into a condition so generated
/// states can be verified at runtime:
///   data  (default) - returns the fixtures below
///   slow            - returns the fixtures after a 6s delay
///   empty           - returns empty collections
///   error           - throws
/// </summary>
public class PortfolioService : IPortfolioService
{
    /// <summary>
    /// Mode file, rewritable while the app runs. This is what makes recovery testable:
    /// drive a component into Error, flip the file to "data", press Retry, and the
    /// transition back proves the command actually re-ran the data path. It defaults to a
    /// fixed temp path so a run launched without environment variables (the App MCP, for
    /// example) can still be driven.
    /// </summary>
    private static string ModeFile =>
        Environment.GetEnvironmentVariable("LAB_MODE_FILE")
        ?? System.IO.Path.Combine(System.IO.Path.GetTempPath(), "componentstateslab-mode.txt");

    /// <summary>
    /// LAB_MODE wins when set, so an env-driven launch is never disturbed by a stale mode
    /// file; the file is the fallback for launches that carry no environment.
    /// </summary>
    private static string Mode
    {
        get
        {
            var env = Environment.GetEnvironmentVariable("LAB_MODE");

            if (!string.IsNullOrEmpty(env))
            {
                return env.ToLowerInvariant();
            }

            try
            {
                if (System.IO.File.Exists(ModeFile))
                {
                    return System.IO.File.ReadAllText(ModeFile).Trim().ToLowerInvariant();
                }
            }
            catch
            {
                // fall through to the default
            }

            return "data";
        }
    }

    /// <summary>Appends one line per service call, so retries can be counted.</summary>
    private static void Trace(string operation)
    {
        var path = Environment.GetEnvironmentVariable("LAB_TRACE");

        if (string.IsNullOrEmpty(path))
        {
            // only trace when the lab is actively driving the app
            if (!System.IO.File.Exists(ModeFile))
            {
                return;
            }

            path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(), "componentstateslab-trace.txt");
        }

        try
        {
            System.IO.File.AppendAllText(path, $"{DateTime.Now:HH:mm:ss.fff}\t{operation}\t{Mode}\n");
        }
        catch
        {
            // tracing must never affect the run
        }
    }

    private static async ValueTask GateAsync(CancellationToken ct)
    {
        await Task.Delay(Mode == "slow" ? 6000 : 400, ct);

        if (Mode == "error")
        {
            throw new InvalidOperationException("Portfolio feed unavailable.");
        }
    }

    public async ValueTask<AccountSummary> GetAccountSummaryAsync(CancellationToken ct = default)
    {
        Trace(nameof(GetAccountSummaryAsync));

        await GateAsync(ct);

        return new AccountSummary("Growth Account", "$184,320.55", "+$1,204.18 today", IsUp: true);
    }

    public async ValueTask<PerformanceMetric?> GetPerformanceAsync(CancellationToken ct = default)
    {
        Trace(nameof(GetPerformanceAsync));

        await GateAsync(ct);

        if (Mode == "empty")
        {
            return null;
        }

        return new PerformanceMetric(
            Label: "Total return",
            Value: "+18.4%",
            Caption: "Trailing 12 months",
            IsUp: true);
    }

    public async ValueTask<IImmutableList<Sector>> GetSectorsAsync(CancellationToken ct = default)
    {
        Trace(nameof(GetSectorsAsync));

        await GateAsync(ct);

        if (Mode == "empty")
        {
            return ImmutableList<Sector>.Empty;
        }

        return ImmutableList.Create(
            new Sector("tech", "Technology", 6),
            new Sector("health", "Healthcare", 3),
            new Sector("energy", "Energy", 2),
            new Sector("fin", "Financials", 4));
    }

    public async ValueTask<IImmutableList<Holding>> GetHoldingsAsync(CancellationToken ct = default)
    {
        Trace(nameof(GetHoldingsAsync));

        await GateAsync(ct);

        if (Mode == "empty")
        {
            return ImmutableList<Holding>.Empty;
        }

        return ImmutableList.Create(
            new Holding("aapl", "AAPL", "Apple Inc.", "$231.42", "+1.2%", IsUp: true),
            new Holding("nvda", "NVDA", "NVIDIA Corp.", "$182.11", "+2.8%", IsUp: true),
            new Holding("msft", "MSFT", "Microsoft Corp.", "$519.30", "-0.4%", IsUp: false),
            new Holding("cost", "COST", "Costco Wholesale", "$942.65", "+0.6%", IsUp: true),
            new Holding("xom", "XOM", "Exxon Mobil", "$118.07", "-1.1%", IsUp: false));
    }

    public async ValueTask<IImmutableList<SignalReading>> GetSignalsAsync(CancellationToken ct = default)
    {
        Trace(nameof(GetSignalsAsync));

        await GateAsync(ct);

        if (Mode == "empty")
        {
            return ImmutableList<SignalReading>.Empty;
        }

        return ImmutableList.Create(
            new SignalReading("Uplink", "42.8", "Mbps", 4),
            new SignalReading("Latency", "18", "ms", 3),
            new SignalReading("Jitter", "2.4", "ms", 5));
    }
}
