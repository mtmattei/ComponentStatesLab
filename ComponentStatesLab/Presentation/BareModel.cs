using ComponentStatesLab.Business;
using ComponentStatesLab.Services;

namespace ComponentStatesLab.Presentation;

/// <summary>
/// Backs the bespoke signal widget. The page itself establishes no state pattern, so the
/// readings are exposed through the same MVUX feed mechanism MainModel already uses -
/// that is the app's existing answer for async state, not a new one introduced here.
/// </summary>
public partial record BareModel
{
    public BareModel(INavigator navigator, IPortfolioService portfolio)
    {
        Portfolio = portfolio;

        Title = "Signal";
        CapturedAt = "Captured 14:22 local";
    }

    private IPortfolioService Portfolio { get; }

    public string Title { get; }

    /// <summary>Rows rendered by the signal widget.</summary>
    public IListFeed<SignalReading> Readings =>
        ListFeed.Async(Portfolio.GetSignalsAsync);

    public string CapturedAt { get; }
}
