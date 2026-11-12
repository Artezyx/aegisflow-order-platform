namespace OrbitField.App;
public partial class App : Application
{
    private readonly MainPage _main;
    public App(MainPage main)
    {
        InitializeComponent();
        _main=main;
    }
    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(_main){Title="OrbitField"};
}
