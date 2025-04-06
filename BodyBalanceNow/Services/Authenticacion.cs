using System;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace BodyBalanceNow.Services
{
    public class Authenticacion
    {
        // Verifica si el usuario ha iniciado sesión
        public static bool UsuarioHaIniciadoSesion()
        {
            // Recupera el ID del usuario desde las preferencias
            var currentUserId = Preferences.Get("current_user_id", -1);

            // Si el ID es -1 (valor por defecto), significa que no hay sesión activa
            return currentUserId != -1;
        }

        // Maneja el cierre de sesión
        public async Task CerrarSesion()
        {
            // Opcional: preguntar al usuario si desea cerrar sesión
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Cerrar sesión",
                "¿Estás seguro que quieres cerrar sesión?",
                "Sí",
                "No");

            if (!confirm)
                return; // No cierra sesión si el usuario elige "No"

            // Elimina el ID del usuario de las preferencias
            Preferences.Remove("current_user_id");

            // Redirige al login
            await Shell.Current.GoToAsync("//login");
        }
    }
}
