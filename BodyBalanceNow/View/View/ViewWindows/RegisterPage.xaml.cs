using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Text.RegularExpressions;
using BodyBalanceNow.View.Components;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace BodyBalanceNow.View.ViewWindows;

public partial class RegisterPage : ContentPage
{
    private readonly UsuarioDatabase _userDatabase; // Base de datos de usuarios
    bool isPasswordVisible = false; // Variable para controlar la visibilidad de la contraseña
    public RegisterPage()
	{
		InitializeComponent();
        _userDatabase = new UsuarioDatabase();
	}

    /// Método para registrar un nuevo usuario
    private async void OnRegisterUser(object sender, EventArgs e)
    {
        string name_user = entryNameUser.Text;
        string email_user = entryEmail.Text;
        string password_user = entryPassword.Text;

        if (string.IsNullOrEmpty(name_user) || string.IsNullOrEmpty(email_user) ||
            string.IsNullOrEmpty(password_user))
        {
            var popup = new CustomPopup("Rellene todos los campos");
            this.ShowPopup(popup);
            return;
        }

        if (!ValidEmail(email_user))
        {
            var popup = new CustomPopup("Recuerde que el correo tiene que contener @tfg.vi");
            this.ShowPopup(popup);
            return;
        }

        if (_userDatabase.UsuarioExiste(email_user))
        {
            var popup = new CustomPopup("El usuario ya existe");
            this.ShowPopup(popup);
            return;
        }

        // Crear el nuevo usuario
        var new_user = new Usuario
        {
            Name = name_user,
            Email = email_user,
            Password = password_user, 

        };

        try
        {
            _userDatabase.RegistrarUsuario(new_user);

            var successPopup = new CustomPopup("Usuario registrado correctamente");
            this.ShowPopup(successPopup);

            ClearEntrys();
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al registrar usuario: {ex.Message}");
        }

    }

    // Limpiar campos después del registro
    private void ClearEntrys()
    {
        entryEmail.Text = string.Empty;
        entryNameUser.Text = string.Empty;
        entryPassword.Text = string.Empty;

    }

    // Método para mostrar/ocultar la contraseña
    private void OnHiddenPassword(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        entryPassword.IsPassword = !isPasswordVisible;
    }

    //  Método para validar el formato del correo electrónico
    private bool ValidEmail(string email)
    {
        string patron = @"^[a-zA-Z0-9._%+-]+@tfg\.vi$";
        return Regex.IsMatch(email, patron);
    }
 
}