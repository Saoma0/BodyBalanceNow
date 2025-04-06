using BodyBalanceNow.Services;
using BCrypt.Net;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace BodyBalanceNow.View;

public partial class LoginPage : ContentPage
{
    private readonly UsuarioDatabase _userDatabase;

    public LoginPage()
    {
        InitializeComponent();
        _userDatabase = new UsuarioDatabase(); // Inicialización del servicio de base de datos
    }

    private bool isPasswordVisible = false;

    // Alterna la visibilidad de la contraseña
    private void OnHiddenPassword(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        entryPassword.IsPassword = !isPasswordVisible;
    }

    // Redirige a la página de registro
    private async void OnLabelTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//registro"); // Navega hacia la página de registro
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

                // Redirige a la página principal
                await Shell.Current.GoToAsync("//main");
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
}
