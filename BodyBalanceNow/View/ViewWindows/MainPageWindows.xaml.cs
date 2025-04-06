namespace BodyBalanceNow.View.ViewWindows;
using BodyBalanceNow.Services;

public partial class MainPageWindows : ContentPage
{
    private readonly Authenticacion _auth;

    public MainPageWindows()
    {
        InitializeComponent();
        _auth = new Authenticacion();
    }

    private void OnCerrarSesion(object sender, EventArgs e)
    {
        _auth.CerrarSesion();
    }
}