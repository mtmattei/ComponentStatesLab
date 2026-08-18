namespace PulseCpuRepro;

/// <summary>
/// Repro modes, chosen by command line so CPU can be sampled without any input:
///   (no args)       storyboard never begun
///   --pulse         Begin() on load, left running
///   --pulse-stop    Begin() on load, Stop() after 3s
///   --pulse-hide    Begin() on load, target collapsed after 3s
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var args = Environment.GetCommandLineArgs();
        var mode = args.FirstOrDefault(a => a.StartsWith("--pulse", StringComparison.Ordinal));

        if (mode is null)
        {
            Status.Text = "storyboard never begun";
            return;
        }

        Pulse.Begin();
        Status.Text = "storyboard running";

        if (mode == "--pulse-stop")
        {
            await Task.Delay(3000);
            Pulse.Stop();
            Status.Text = "storyboard stopped";
        }
        else if (mode == "--pulse-hide")
        {
            await Task.Delay(3000);
            Block.Visibility = Visibility.Collapsed;
            Status.Text = "running, target collapsed";
        }
    }

    private void OnBegin(object sender, RoutedEventArgs e) => Pulse.Begin();

    private void OnStop(object sender, RoutedEventArgs e) => Pulse.Stop();

    private void OnCollapse(object sender, RoutedEventArgs e)
        => Block.Visibility = Visibility.Collapsed;
}
