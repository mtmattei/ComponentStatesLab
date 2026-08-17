using ComponentStatesLab.Business;
using ComponentStatesLab.Services;

namespace ComponentStatesLab.Presentation;

public partial record MainModel
{
    private readonly INavigator _navigator;

    public MainModel(
        IStringLocalizer localizer,
        IOptions<AppConfig> appInfo,
        INavigator navigator,
        IPortfolioService portfolio)
    {
        _navigator = navigator;
        Portfolio = portfolio;

        Title = "Portfolio";

        Performance = new PerformanceMetric(
            Label: "Total return",
            Value: "+18.4%",
            Caption: "Trailing 12 months",
            IsUp: true);
    }

    private IPortfolioService Portfolio { get; }

    public string Title { get; }

    /// <summary>Headline metric for the performance card. Plain data, always present.</summary>
    public PerformanceMetric Performance { get; }

    /// <summary>Backs the account header widget, which is wired through FeedView.</summary>
    public IFeed<AccountSummary> Account =>
        Feed.Async(Portfolio.GetAccountSummaryAsync);

    /// <summary>Sector filters rendered as a chip bar.</summary>
    public IListFeed<Sector> Sectors =>
        ListFeed.Async(Portfolio.GetSectorsAsync);

    /// <summary>Rows rendered by the holdings list.</summary>
    public IListFeed<Holding> Holdings =>
        ListFeed.Async(Portfolio.GetHoldingsAsync);

    public async ValueTask GoToSignals(CancellationToken ct)
    {
        await _navigator.NavigateViewModelAsync<BareModel>(this, cancellation: ct);
    }
}
