namespace BodyBalanceNow.View.ViewAndroid;

using System.Diagnostics;
using BodyBalanceNow.Services;

public partial class MainPageAndroid : ContentPage
{
    private readonly Authenticacion _auth;
    private readonly UsuarioDatabaseAndroid _userDatabase;
    private bool isPasswordVisible = false;

    public MainPageAndroid()
    {
        InitializeComponent();
        _auth = new Authenticacion();
        _userDatabase = new UsuarioDatabaseAndroid(); // Inicializaci�n del servicio de base de datos
        if (Preferences.ContainsKey("current_user_id"))
        {
            btnIniciarSesion.IsVisible = false;
            btnCerrarSesion.IsVisible = true;
            btnBorrarusuarion.IsVisible = true;
        }
    }

    private async void OnCerrarSesion(object sender, EventArgs e)
    {
        bool confirmacion = await DisplayAlert("Cerrar sesi�n", "�Est�s seguro de que quieres cerrar sesi�n?", "S�", "No");

        if (confirmacion)
        {
            _auth.CerrarSesion();

            btnIniciarSesion.IsVisible = true;
            btnCerrarSesion.IsVisible = false;
            btnBorrarusuarion.IsVisible = false;

            entryEmail.Text = string.Empty;
            entryPassword.Text = string.Empty;

        }
    }


    // Alterna la visibilidad de la contrase�a
    private void OnHiddenPassword(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        entryPassword.IsPassword = !isPasswordVisible;
    }

    // Redirige a la p�gina de registro
    private async void OnLabelTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("RegisterPage"); // Navega hacia la p�gina de registro
    }

    // Maneja el inicio de sesi�n
    private async void OnLoginUser(object sender, EventArgs e)
    {
        string email_user = entryEmail.Text?.Trim();
        string password_user = entryPassword.Text?.Trim();

        // Validaciones iniciales
        if (string.IsNullOrEmpty(email_user) || string.IsNullOrEmpty(password_user))
        {
            await DisplayAlert("Error", "Por favor rellene todos los campos", "Cerrar");
            return;
        }

        try
        {
            // Verifica si el usuario existe y valida la contrase�a
            var user = _userDatabase.Login(email_user, password_user);
            if (user != null)
            {
                await DisplayAlert("�xito", "Inicio de sesi�n exitoso", "Cerrar");

                // Guarda el ID del usuario en las preferencias
                Preferences.Set("current_user_id", user.Id);

                // Oculta bot�n de login y muestra el de cerrar sesi�n
                btnIniciarSesion.IsVisible = false;
                btnCerrarSesion.IsVisible = true;
                btnBorrarusuarion.IsVisible = true;

                // Limpia los campos de entrada
                entryEmail.Text = string.Empty;
                entryPassword.Text = string.Empty;

            }
            else
            {
                await DisplayAlert("Error", "Correo o contrase�a incorrectos", "Cerrar");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurri� un problema al iniciar sesi�n: {ex.Message}", "Cerrar");
        }
    }


    private void OnBoton(object sender, EventArgs e)
    {
        Debug.WriteLine(Preferences.Get("current_user_id", -1));
    }

    private async void OnBorrarUsuario(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Borrar cuenta", "�Seguro que quieres eliminar tu cuenta y todos tus datos?", "S�", "No");
        if (!confirm)
            return;

        int userId = Preferences.Get("current_user_id", -1);

        if (userId == -1)
        {
            await DisplayAlert("Error", "No se encontr� un usuario v�lido en sesi�n.", "Cerrar");
            return;
        }

        try
        {
            await _userDatabase.EliminarUsuarioPorId(userId);

            Preferences.Remove("current_user_id");

            // Restablece estado visual
            btnIniciarSesion.IsVisible = true;
            btnCerrarSesion.IsVisible = false;
            btnBorrarusuarion.IsVisible = false;
            entryEmail.Text = string.Empty;
            entryPassword.Text = string.Empty;

            await DisplayAlert("Cuenta eliminada", "Tu cuenta y todos tus datos han sido eliminados.", "Aceptar");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo eliminar el usuario: {ex.Message}", "Cerrar");
        }
    }


}