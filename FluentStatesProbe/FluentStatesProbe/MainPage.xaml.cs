using FluentStatesProbe.Services;
using FluentStatesProbe.ViewModels;

namespace FluentStatesProbe;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = new(new OrderService());

    // Skeleton pulse. A RepeatBehavior="Forever" storyboard is unusable for this
    // on Skia desktop: it pumps compositor frames at display refresh for the life
    // of the app, and Stop() does not reclaim the cost once it has run. A timer
    // produces exactly one frame per tick and stops cleanly with the loading state.
    private readonly DispatcherTimer _pulseTimer = new() { Interval = TimeSpan.FromMilliseconds(80) };
    private double _pulsePhase;

    public MainPage()
    {
        this.InitializeComponent();

        _pulseTimer.Tick += (_, _) =>
        {
            _pulsePhase += 0.08 / 1.8; // 1.8s period
            LoadingSkeleton.Opacity = 0.675 + 0.225 * Math.Sin(_pulsePhase * 2 * Math.PI);
        };

        ViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsLoading))
            {
                UpdatePulse();
            }
        };

        Loaded += (_, _) => UpdatePulse();
        Unloaded += (_, _) => _pulseTimer.Stop();
    }

    private void UpdatePulse()
    {
        if (ViewModel.IsLoading)
        {
            _pulseTimer.Start();
        }
        else
        {
            _pulseTimer.Stop();
            LoadingSkeleton.Opacity = 1;
        }
    }
}
