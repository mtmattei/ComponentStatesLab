using ComponentStatesLab.Business;
using ComponentStatesLab.Services;

namespace ComponentStatesLab.Presentation;

/// <summary>
/// Backs the bespoke signal widget. Deliberately carries no feed, no FeedView, and no
/// state handling of any kind - this page establishes no house pattern.
/// </summary>
public partial record BareModel
{
    public BareModel(INavigator navigator, IPortfolioService portfolio)
    {
        Title = "Signal";

        Readings = ImmutableList.Create(
            new SignalReading("Uplink", "42.8", "Mbps", 4),
            new SignalReading("Latency", "18", "ms", 3),
            new SignalReading("Jitter", "2.4", "ms", 5));

        CapturedAt = "Captured 14:22 local";
    }

    public string Title { get; }

    public IImmutableList<SignalReading> Readings { get; }

    public string CapturedAt { get; }
}
