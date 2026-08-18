namespace ProductCardApp.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        // DataContext is supplied by Uno.Extensions.Navigation via ViewMap<MainPage, MainModel>
        // MainModel requires IProductService from DI — no parameterless constructor available
    }
}
