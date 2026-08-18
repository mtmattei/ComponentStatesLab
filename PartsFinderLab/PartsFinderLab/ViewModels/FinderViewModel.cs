using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PartsFinderLab.Models;
using PartsFinderLab.Services;

namespace PartsFinderLab.ViewModels;

/// <summary>Condition of a data-fed region. Data is the populated state.</summary>
public enum LoadState
{
    Loading,
    Data,
    Empty,
    Error,
}

public sealed class FinderViewModel : INotifyPropertyChanged
{
    // Skeleton-flash guard: don't show the skeleton for a load that resolves fast,
    // and once it is shown, hold it long enough that it never strobes.
    private const int SkeletonDelayMs = 300;
    private const int SkeletonMinVisibleMs = 500;

    private readonly PartsService _service = new();

    private WarehouseInfo? _warehouse;
    private string _searchText = string.Empty;

    private LoadState _warehouseState = LoadState.Loading;
    private LoadState _resultsState = LoadState.Loading;
    private string _emptyMessage = string.Empty;
    private bool _showClearSearch;
    private bool _canSearch = true;
    private bool _canRetryWarehouse = true;

    private int _searchGeneration;
    private int _warehouseGeneration;
    private string? _lastQuery;

    public ObservableCollection<Part> Results { get; } = new();

    public WarehouseInfo? Warehouse
    {
        get => _warehouse;
        private set => Set(ref _warehouse, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => Set(ref _searchText, value);
    }

    public LoadState WarehouseState
    {
        get => _warehouseState;
        private set
        {
            if (Set(ref _warehouseState, value))
            {
                Raise(nameof(IsAnyLoading));
            }
        }
    }

    public LoadState ResultsState
    {
        get => _resultsState;
        private set
        {
            if (Set(ref _resultsState, value))
            {
                Raise(nameof(IsAnyLoading));
            }
        }
    }

    /// <summary>Drives the skeleton opacity pulse in the view.</summary>
    public bool IsAnyLoading =>
        WarehouseState == LoadState.Loading || ResultsState == LoadState.Loading;

    public string EmptyMessage
    {
        get => _emptyMessage;
        private set => Set(ref _emptyMessage, value);
    }

    /// <summary>True only for the "none matched" empty, where a way back is useful.</summary>
    public bool ShowClearSearch
    {
        get => _showClearSearch;
        private set => Set(ref _showClearSearch, value);
    }

    public bool CanSearch
    {
        get => _canSearch;
        private set => Set(ref _canSearch, value);
    }

    public bool CanRetryWarehouse
    {
        get => _canRetryWarehouse;
        private set => Set(ref _canRetryWarehouse, value);
    }

    public FinderViewModel()
    {
        _ = LoadWarehouseAsync();
        _ = SearchAsync();
    }

    public async void Search() => await SearchAsync();

    public async void RetryWarehouse() => await LoadWarehouseAsync();

    public async void ClearSearch()
    {
        SearchText = string.Empty;
        await SearchAsync();
    }

    private async Task LoadWarehouseAsync()
    {
        if (!CanRetryWarehouse)
        {
            // Re-entry guard: safe to click Retry twice.
            return;
        }

        var generation = ++_warehouseGeneration;
        CanRetryWarehouse = false;

        var call = _service.GetWarehouseAsync();
        var shownAt = await ShowSkeletonIfSlowAsync(call, () => WarehouseState = LoadState.Loading);

        try
        {
            var warehouse = await call;
            if (generation != _warehouseGeneration)
            {
                return;
            }

            await HoldSkeletonAsync(shownAt);
            Warehouse = warehouse;
            WarehouseState = LoadState.Data;
        }
        catch (Exception)
        {
            if (generation != _warehouseGeneration)
            {
                return;
            }

            await HoldSkeletonAsync(shownAt);
            WarehouseState = LoadState.Error;
        }
        finally
        {
            if (generation == _warehouseGeneration)
            {
                CanRetryWarehouse = true;
            }
        }
    }

    private async Task SearchAsync()
    {
        if (!CanSearch)
        {
            // Re-entry guard: Search and Retry are safe to click twice.
            return;
        }

        var query = SearchText ?? string.Empty;
        var generation = ++_searchGeneration;
        CanSearch = false;

        // Reload is not first load: re-running the same query while data is on screen
        // keeps the rows visible instead of blanking them into a skeleton.
        var isReload = ResultsState == LoadState.Data && query == _lastQuery;

        var call = _service.SearchPartsAsync(query);
        var shownAt = isReload
            ? (long?)null
            : await ShowSkeletonIfSlowAsync(call, () => ResultsState = LoadState.Loading);

        try
        {
            var results = await call;
            if (generation != _searchGeneration)
            {
                return;
            }

            await HoldSkeletonAsync(shownAt);

            Results.Clear();
            foreach (var part in results)
            {
                Results.Add(part);
            }

            _lastQuery = query;

            if (results.Count > 0)
            {
                ResultsState = LoadState.Data;
            }
            else if (string.IsNullOrWhiteSpace(query))
            {
                // Nothing in the catalogue at all: nothing in this app creates parts,
                // so there is no call to action here.
                EmptyMessage = "No parts in the catalogue.";
                ShowClearSearch = false;
                ResultsState = LoadState.Empty;
            }
            else
            {
                // None matched: offer the way back, not a "create" action.
                EmptyMessage = $"No parts match “{query}”.";
                ShowClearSearch = true;
                ResultsState = LoadState.Empty;
            }
        }
        catch (Exception)
        {
            if (generation != _searchGeneration)
            {
                return;
            }

            await HoldSkeletonAsync(shownAt);
            ResultsState = LoadState.Error;
        }
        finally
        {
            if (generation == _searchGeneration)
            {
                CanSearch = true;
            }
        }
    }

    /// <summary>
    /// Waits <see cref="SkeletonDelayMs"/>; if the call is still in flight, switches to the
    /// skeleton and returns the timestamp it appeared. Returns null when no skeleton was shown.
    /// </summary>
    private static async Task<long?> ShowSkeletonIfSlowAsync(Task call, Action showSkeleton)
    {
        var delay = Task.Delay(SkeletonDelayMs);
        var first = await Task.WhenAny(call, delay).ConfigureAwait(true);

        if (first == delay && !call.IsCompleted)
        {
            showSkeleton();
            return Environment.TickCount64;
        }

        return null;
    }

    private static async Task HoldSkeletonAsync(long? shownAt)
    {
        if (shownAt is not { } start)
        {
            return;
        }

        var remaining = SkeletonMinVisibleMs - (int)(Environment.TickCount64 - start);
        if (remaining > 0)
        {
            await Task.Delay(remaining).ConfigureAwait(true);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        Raise(name);
        return true;
    }

    private void Raise(string? name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
