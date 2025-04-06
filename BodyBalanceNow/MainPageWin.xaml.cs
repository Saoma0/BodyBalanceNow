namespace BodyBalanceNow;
using BodyBalanceNow.Services;

public partial class MainPage : ContentPage
{
    private readonly Authenticacion _auth;

    public MainPage()
    {
        InitializeComponent();
        _auth = new Authenticacion(); // Inicializa la clase Authenticación
    }

    private void OnCerrarSesion(object sender, EventArgs e)
    {
        _auth.CerrarSesion(); // Llama al método para cerrar sesión
    }
}
