namespace BodyBalanceNow.View.ViewAndroid;
using BodyBalanceNow.Services;

public partial class MainPageAndroid : ContentPage
{
    private readonly Authenticacion _auth;

    public MainPageAndroid()
    {
        InitializeComponent();
        _auth = new Authenticacion();
    }

    private void OnCerrarSesion(object sender, EventArgs e)
    {
        _auth.CerrarSesion();
    }
}