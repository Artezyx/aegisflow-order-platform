using OrbitField.App.ViewModels;
namespace OrbitField.App;
public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel model)
    {
        InitializeComponent();
        BindingContext=model;
    }
}
