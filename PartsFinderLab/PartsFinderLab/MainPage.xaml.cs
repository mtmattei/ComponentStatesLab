using System.ComponentModel;
using PartsFinderLab.ViewModels;

namespace PartsFinderLab;

public sealed partial class MainPage : Page
{
    // The skeleton pulse is driven by a DispatcherTimer, not a RepeatBehavior="Forever"
    // storyboard: on Uno Skia desktop a forever storyboard pumps compositor frames for the
    // life of the process and Stop() does not reclaim the cost.
    private const double PulsePeriodMs = 1800;
    private const double PulseIntervalMs = 80;

    private readonly DispatcherTimer _pulseTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(PulseIntervalMs),
    };

    private double _pulsePhase;

    public FinderViewModel ViewModel { get; } = new();

    public MainPage()
    {
        this.InitializeComponent();

        _pulseTimer.Tick += OnPulseTick;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        Unloaded += OnUnloaded;

        SyncPulse();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _pulseTimer.Stop();
        _pulseTimer.Tick -= OnPulseTick;
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        Unloaded -= OnUnloaded;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(FinderViewModel.IsAnyLoading))
        {
            SyncPulse();
        }
    }

    private void SyncPulse()
    {
        if (ViewModel.IsAnyLoading)
        {
            if (!_pulseTimer.IsEnabled)
            {
                _pulsePhase = 0;
                _pulseTimer.Start();
            }
        }
        else
        {
            _pulseTimer.Stop();
            WarehouseSkeleton.Opacity = 1;
            ResultsSkeleton.Opacity = 1;
        }
    }

    private void OnPulseTick(object? sender, object e)
    {
        _pulsePhase += PulseIntervalMs / PulsePeriodMs * 2 * Math.PI;

        // 0.45 -> 0.90 -> 0.45
        var opacity = 0.45 + 0.45 * (0.5 + 0.5 * Math.Sin(_pulsePhase));

        WarehouseSkeleton.Opacity = opacity;
        ResultsSkeleton.Opacity = opacity;
    }
}
