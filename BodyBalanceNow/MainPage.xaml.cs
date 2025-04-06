namespace BodyBalanceNow;
using BodyBalanceNow.Services;

public partial class MainPage : ContentPage
{
    private readonly Authenticacion _auth;

    public MainPage()
    {
        InitializeComponent();
        _auth = new Authenticacion(); // Inicializa la clase Authenticaci�n
    }

    private void OnCerrarSesion(object sender, EventArgs e)
    {
        _auth.CerrarSesion(); // Llama al m�todo para cerrar sesi�n
    }
}
