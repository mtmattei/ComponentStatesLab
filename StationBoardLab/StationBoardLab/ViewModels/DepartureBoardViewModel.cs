using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StationBoardLab.Models;
using StationBoardLab.Services;

namespace StationBoardLab.ViewModels;

public enum BoardState
{
    Loading,
    Data,
    Empty,
    Error,
}

public sealed class DepartureBoardViewModel : INotifyPropertyChanged
{
    // The skeleton only appears if the load outlasts this, so a fast response
    // never flashes it; once shown it is held long enough not to strobe.
    private const int SkeletonDelayMs = 300;
    private const int SkeletonMinVisibleMs = 500;

    private readonly DepartureService _service = new();

    private long _skeletonShownAtMs;
    private bool _isBusy;
    private BoardState _state = BoardState.Loading;

    public ObservableCollection<Departure> Departures { get; } = new();

    public BoardState State
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            Raise();
        }
    }

    // Bound to IsEnabled on both Refresh and Retry: a load in flight disables
    // them, which is the re-entry guard and the reload busy hint at once.
    public bool IsIdle => !_isBusy;

    public DepartureBoardViewModel()
    {
        _ = LoadAsync();
    }

    // Bound to the panel header's Refresh button and to the error state's Retry.
    public async void Refresh() => await LoadAsync();

    private async Task LoadAsync()
    {
        if (_isBusy)
        {
            return;
        }

        SetBusy(true);

        // First load and recovery show the skeleton; a refresh over data on
        // screen keeps that data visible instead of blanking it.
        var showSkeleton = State != BoardState.Data;
        using var skeletonDelay = new CancellationTokenSource();

        if (showSkeleton)
        {
            _ = ShowSkeletonAfterDelayAsync(skeletonDelay.Token);
        }

        try
        {
            var departures = await _service.GetDeparturesAsync();
            skeletonDelay.Cancel();
            await HoldSkeletonAsync();

            Departures.Clear();
            foreach (var departure in departures)
            {
                Departures.Add(departure);
            }

            State = departures.Count == 0 ? BoardState.Empty : BoardState.Data;
        }
        catch (Exception)
        {
            skeletonDelay.Cancel();
            await HoldSkeletonAsync();

            Departures.Clear();
            State = BoardState.Error;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ShowSkeletonAfterDelayAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(SkeletonDelayMs, ct);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (ct.IsCancellationRequested)
        {
            return;
        }

        _skeletonShownAtMs = Environment.TickCount64;
        Departures.Clear();
        State = BoardState.Loading;
    }

    private async Task HoldSkeletonAsync()
    {
        if (State != BoardState.Loading)
        {
            return;
        }

        var visibleFor = Environment.TickCount64 - _skeletonShownAtMs;
        if (visibleFor < SkeletonMinVisibleMs)
        {
            await Task.Delay((int)(SkeletonMinVisibleMs - visibleFor));
        }
    }

    private void SetBusy(bool value)
    {
        if (_isBusy == value)
        {
            return;
        }

        _isBusy = value;
        Raise(nameof(IsIdle));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Raise([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
