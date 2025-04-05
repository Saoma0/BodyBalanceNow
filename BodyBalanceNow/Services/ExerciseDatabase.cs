

using BodyBalanceNow.Models;
using System.Diagnostics;

namespace BodyBalanceNow.Services
{
    public class ExerciseDatabase
    {
        private ConfigConnection config;
        private string connectionString;

        // Constructor para inicializar los valores
        public ExerciseDatabase()
        {
            config = ConfigConnection.CargarDesdeJson(); // Carga la configuración
            connectionString = config.GetConnectionString(); // Genera el string de conexión
        }

        // Ejemplo de método para usar la conexión
        public void MostrarConnectionString()
        {
            Debug.WriteLine($"Conexión lista con: {connectionString}");
        }
    }
}
