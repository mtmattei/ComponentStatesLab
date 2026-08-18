using System.ComponentModel;
using StationBoardLab.ViewModels;

namespace StationBoardLab.Controls;

public sealed partial class DepartureBoard : UserControl
{
    // The skeleton pulse runs off a timer, not a forever-storyboard: on Uno
    // Skia desktop a repeating storyboard keeps the compositor running for the
    // life of the app even after it is stopped.
    private const double PulseMin = 0.45;
    private const double PulseMax = 0.9;
    private const double PulseStep = 0.04;

    private readonly DispatcherTimer _pulseTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(80),
    };

    private double _pulseOpacity = PulseMax;
    private double _pulseDirection = -PulseStep;

    public DepartureBoardViewModel ViewModel { get; } = new();

    public DepartureBoard()
    {
        this.InitializeComponent();

        _pulseTimer.Tick += OnPulseTick;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        Loaded += (_, _) => ApplyState();
        Unloaded += (_, _) => _pulseTimer.Stop();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DepartureBoardViewModel.State))
        {
            ApplyState();
        }
    }

    private void ApplyState()
    {
        var stateName = ViewModel.State switch
        {
            BoardState.Loading => "Loading",
            BoardState.Empty => "Empty",
            BoardState.Error => "Error",
            _ => "Data",
        };

        VisualStateManager.GoToState(this, stateName, false);

        if (ViewModel.State == BoardState.Loading)
        {
            StartPulse();
        }
        else
        {
            StopPulse();
        }
    }

    private void StartPulse()
    {
        if (_pulseTimer.IsEnabled)
        {
            return;
        }

        _pulseOpacity = PulseMax;
        _pulseDirection = -PulseStep;
        SkeletonLayer.Opacity = _pulseOpacity;
        _pulseTimer.Start();
    }

    private void StopPulse()
    {
        _pulseTimer.Stop();
        SkeletonLayer.Opacity = 1;
    }

    private void OnPulseTick(object? sender, object e)
    {
        _pulseOpacity += _pulseDirection;

        if (_pulseOpacity <= PulseMin)
        {
            _pulseOpacity = PulseMin;
            _pulseDirection = PulseStep;
        }
        else if (_pulseOpacity >= PulseMax)
        {
            _pulseOpacity = PulseMax;
            _pulseDirection = -PulseStep;
        }

        SkeletonLayer.Opacity = _pulseOpacity;
    }
}
