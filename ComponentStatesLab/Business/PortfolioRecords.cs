namespace ComponentStatesLab.Business;

/// <summary>Header summary shown by the account FeedView widget.</summary>
public partial record AccountSummary(string AccountName, string TotalValue, string DayChange, bool IsUp);

/// <summary>Single headline metric shown in the performance card.</summary>
public partial record PerformanceMetric(string Label, string Value, string Caption, bool IsUp);

/// <summary>Sector filter shown as a chip.</summary>
public partial record Sector(string Id, string Name, int Count);

/// <summary>One row of the holdings list.</summary>
public partial record Holding(string Id, string Ticker, string Company, string Price, string Change, bool IsUp);

/// <summary>Reading rendered by the bespoke signal widget on BarePage.</summary>
public partial record SignalReading(string Channel, string Value, string Unit, int Strength)
{
    /// <summary>Width of the filled portion of the strength bar, against a 160px track.</summary>
    public double FillWidth => Strength / 5d * 160d;
}
