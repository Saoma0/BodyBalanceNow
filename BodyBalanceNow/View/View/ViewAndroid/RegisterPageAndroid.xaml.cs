using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Text.RegularExpressions;
using BodyBalanceNow.View.Components;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace BodyBalanceNow.View.ViewAndroid;

public partial class RegisterPageAndroid : ContentPage
{
    private readonly UsuarioDatabaseAndroid _userDatabase; // Base de datos de usuarios
    bool isPasswordVisible = false; // Variable para controlar la visibilidad de la contrase�a
    public RegisterPageAndroid()
	{
		InitializeComponent();
        _userDatabase = new UsuarioDatabaseAndroid();
	}

    /// M�todo para registrar un nuevo usuario
    private async void OnRegisterUser(object sender, EventArgs e)
    {
        // Obt�n los valores de los campos
        string name_user = entryNameUser.Text;
        string email_user = entryEmail.Text;
        string password_user = entryPassword.Text;


        // Validaciones iniciales
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
            _userDatabase.RegistrarUsuarioAndroid(new_user);
            var popup = new CustomPopup("Usuario registrado correctamente");
            this.ShowPopup(popup);
            ClearEntrys();

            // Navegar a la p�gina de inicio de sesi�n
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al registrar usuario: {ex.Message}");
        }
    }

    // Limpiar campos despu�s del registro
    private void ClearEntrys()
    {
        entryEmail.Text = string.Empty;
        entryNameUser.Text = string.Empty;
        entryPassword.Text = string.Empty;

    }
   
    // M�todo para alternar la visibilidad de la contrase�a
    private void OnHiddenPassword(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        entryPassword.IsPassword = !isPasswordVisible;
    }

    // M�todo para validar el formato del correo electr�nico
    private bool ValidEmail(string email)
    {
        string patron = @"^[a-zA-Z0-9._%+-]+@tfg\.vi$";
        return Regex.IsMatch(email, patron);
    }



}