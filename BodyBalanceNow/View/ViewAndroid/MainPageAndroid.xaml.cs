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
        _userDatabase = new UsuarioDatabaseAndroid(); // Inicialización del servicio de base de datos
        if (Preferences.ContainsKey("current_user_id"))
        {
            btnIniciarSesion.IsVisible = false;
            btnCerrarSesion.IsVisible = true;
            btnBorrarusuarion.IsVisible = true;
        }
    }

    private async void OnCerrarSesion(object sender, EventArgs e)
    {
        bool confirmacion = await DisplayAlert("Cerrar sesión", "¿Estás seguro de que quieres cerrar sesión?", "Sí", "No");

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


    // Alterna la visibilidad de la contraseña
    private void OnHiddenPassword(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        entryPassword.IsPassword = !isPasswordVisible;
    }

    // Redirige a la página de registro
    private async void OnLabelTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("RegisterPage"); // Navega hacia la página de registro
    }

    // Maneja el inicio de sesión
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
            // Verifica si el usuario existe y valida la contraseña
            var user = _userDatabase.Login(email_user, password_user);
            if (user != null)
            {
                await DisplayAlert("Éxito", "Inicio de sesión exitoso", "Cerrar");

                // Guarda el ID del usuario en las preferencias
                Preferences.Set("current_user_id", user.Id);

                // Oculta botón de login y muestra el de cerrar sesión
                btnIniciarSesion.IsVisible = false;
                btnCerrarSesion.IsVisible = true;
                btnBorrarusuarion.IsVisible = true;

                // Limpia los campos de entrada
                entryEmail.Text = string.Empty;
                entryPassword.Text = string.Empty;

            }
            else
            {
                await DisplayAlert("Error", "Correo o contraseña incorrectos", "Cerrar");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un problema al iniciar sesión: {ex.Message}", "Cerrar");
        }
    }


    private void OnBoton(object sender, EventArgs e)
    {
        Debug.WriteLine(Preferences.Get("current_user_id", -1));
    }

    private async void OnBorrarUsuario(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Borrar cuenta", "¿Seguro que quieres eliminar tu cuenta y todos tus datos?", "Sí", "No");
        if (!confirm)
            return;

        int userId = Preferences.Get("current_user_id", -1);

        if (userId == -1)
        {
            await DisplayAlert("Error", "No se encontró un usuario válido en sesión.", "Cerrar");
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