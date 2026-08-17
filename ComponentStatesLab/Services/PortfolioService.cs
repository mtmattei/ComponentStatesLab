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
    private static string Mode =>
        Environment.GetEnvironmentVariable("LAB_MODE")?.ToLowerInvariant() ?? "data";

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
        await GateAsync(ct);

        return new AccountSummary("Growth Account", "$184,320.55", "+$1,204.18 today", IsUp: true);
    }

    public async ValueTask<IImmutableList<Sector>> GetSectorsAsync(CancellationToken ct = default)
    {
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
