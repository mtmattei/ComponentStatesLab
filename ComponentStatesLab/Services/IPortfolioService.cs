using ComponentStatesLab.Business;

namespace ComponentStatesLab.Services;

public interface IPortfolioService
{
    ValueTask<AccountSummary> GetAccountSummaryAsync(CancellationToken ct = default);

    ValueTask<IImmutableList<Sector>> GetSectorsAsync(CancellationToken ct = default);

    ValueTask<IImmutableList<Holding>> GetHoldingsAsync(CancellationToken ct = default);

    ValueTask<IImmutableList<SignalReading>> GetSignalsAsync(CancellationToken ct = default);
}
