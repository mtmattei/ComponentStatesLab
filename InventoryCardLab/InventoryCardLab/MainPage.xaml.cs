namespace InventoryCardLab;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        DataContext = new MainViewModel(new InventoryService());
    }
}
