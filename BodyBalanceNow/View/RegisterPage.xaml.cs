using BodyBalanceNow.Models;
using BodyBalanceNow.Services;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Text.RegularExpressions;

namespace BodyBalanceNow.View;

public partial class RegisterPage : ContentPage
{
    private readonly UsuarioDatabase _userDatabase;
	public RegisterPage()
	{
		InitializeComponent();
        _userDatabase = new UsuarioDatabase();
	}


    private async void OnRegisterUser(object sender, EventArgs e)
    {
        // Obtén los valores de los campos
        string name_user = entryNameUser.Text;
        string email_user = entryEmail.Text;
        string password_user = entryPassword.Text;


        // Validaciones iniciales
        if (string.IsNullOrEmpty(name_user) || string.IsNullOrEmpty(email_user) ||
            string.IsNullOrEmpty(password_user))
        {
            await DisplayAlert("Error", "Rellene todos los campos", "Cerrar");
            return;
        }

        if (!ValidEmail(email_user))
        {
            await DisplayAlert("Error", "Recuerde que el correo tiene que contener @tfg.vi", "Cerrar");
            return;
        }

        if (_userDatabase.UsuarioExiste(email_user))
        {
            await DisplayAlert("Error", "El usuario ya existe", "Cerrar");
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
            await DisplayAlert("Éxito", "Usuario registrado correctamente", "Cerrar");
            ClearEntrys();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ha ocurrido un error: {ex.Message}", "Cerrar");
        }
    }

    // Limpiar campos después del registro
    private void ClearEntrys()
    {
        entryEmail.Text = string.Empty;
        entryNameUser.Text = string.Empty;
        entryPassword.Text = string.Empty;

    }


    bool isPasswordVisible = false;
    private void OnHiddenPassword(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        entryPassword.IsPassword = !isPasswordVisible;
    }

    private bool ValidEmail(string email)
    {
        string patron = @"^[a-zA-Z0-9._%+-]+@tfg\.vi$";
        return Regex.IsMatch(email, patron);
    }

  
}