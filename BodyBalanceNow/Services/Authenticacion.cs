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
            Preferences.Remove("current_user_id");
        }
    }
}
