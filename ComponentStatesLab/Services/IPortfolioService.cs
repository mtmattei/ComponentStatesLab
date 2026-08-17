using ComponentStatesLab.Business;

namespace ComponentStatesLab.Services;

public interface IPortfolioService
{
    ValueTask<AccountSummary> GetAccountSummaryAsync(CancellationToken ct = default);

    /// <summary>Returns null in "empty" mode, which the metric card renders as its None state.</summary>
    ValueTask<PerformanceMetric?> GetPerformanceAsync(CancellationToken ct = default);

    ValueTask<IImmutableList<Sector>> GetSectorsAsync(CancellationToken ct = default);

    ValueTask<IImmutableList<Holding>> GetHoldingsAsync(CancellationToken ct = default);

    ValueTask<IImmutableList<SignalReading>> GetSignalsAsync(CancellationToken ct = default);
}
