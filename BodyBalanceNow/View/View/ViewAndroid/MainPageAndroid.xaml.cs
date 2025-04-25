
using System.Diagnostics;
using BodyBalanceNow.Services;
using BodyBalanceNow.View.Components;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace BodyBalanceNow.View.ViewAndroid;
public partial class MainPageAndroid : ContentPage
{
    private readonly Authenticacion _auth; // Servicio de autenticaci�n
    private readonly UsuarioDatabaseAndroid _userDatabase; //   Servicio de base de datos
    private bool isPasswordVisible = false; // Variable para controlar la visibilidad de la contrase�a

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

    // Verifica si el usuario ha iniciado sesi�n al cargar la p�gina
    private async void OnCerrarSesion(object sender, EventArgs e)
    {
        var popup = new ConfirmPopup("�Est�s seguro de que quieres cerrar sesi�n?");
        bool? resultado = await this.ShowPopupAsync(popup) as bool?;
        bool confirmacion = resultado == true;

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
            var popup = new CustomPopup("Por favor rellene todos los campos");
            this.ShowPopup(popup);
            return;
        }

        try
        {
            // Verifica si el usuario existe y valida la contrase�a
            var user = _userDatabase.Login(email_user, password_user);
            if (user != null)
            {
                var popup = new CustomPopup("Inicio de sesi�n exitoso");
                this.ShowPopup(popup);

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
                var popup = new CustomPopup("Correo o contrase�a incorrectos");
                this.ShowPopup(popup);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    // Maneja el evento de clic en el bot�n de prueba
    private void OnBoton(object sender, EventArgs e)
    {
        Debug.WriteLine(Preferences.Get("current_user_id", -1));
    }

    // Redirige a la p�gina de inicio
    private async void OnBorrarUsuario(object sender, EventArgs e)
    {
        var popup = new ConfirmPopup("�Seguro que quieres eliminar tu cuenta y todos tus datos?");
        bool? resultado = await this.ShowPopupAsync(popup) as bool?;
        bool confirm = resultado == true;

        if (!confirm)
            return;

        int userId = Preferences.Get("current_user_id", -1);

        if (userId == -1)
        {
            var popup2 = new CustomPopup("No se encontr� un usuario v�lido en sesi�n.");
            this.ShowPopup(popup2);
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

            var popup3 = new CustomPopup("Tu cuenta y todos tus datos han sido eliminados.");
            this.ShowPopup(popup3);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al eliminar el usuario: {ex.Message}");
        }
    }


}