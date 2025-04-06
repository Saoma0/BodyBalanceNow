using BodyBalanceNow.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace BodyBalanceNow.Services
{
    public class ExerciseDatabase
    {
        private string connectionString = "Server=localhost;Database=tfgdatabase;User Id = root; Password=S@6493483!!!!;";

        // Método para guardar el progreso del IMC
        public async Task GuardarProgresoIMC(ProgresoIMC progreso)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "INSERT INTO ProgresoIMC (FechaRegistro, IMC, IdUser) VALUES (@Fecha, @IMC, @IdUser)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Fecha", progreso.FechaRegistro);
            command.Parameters.AddWithValue("@IMC", progreso.IMC);
            command.Parameters.AddWithValue("@IdUser", progreso.IdUser);

            await command.ExecuteNonQueryAsync();
        }

        // Método para obtener el historial de IMC PRUEBAS
        public async Task<List<ProgresoIMC>> ObtenerHistorialIMC(int idUsuario)
        {
            var lista = new List<ProgresoIMC>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT FechaRegistro, IMC FROM ProgresoIMC WHERE IdUser = @IdUser ORDER BY FechaRegistro DESC";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdUser", idUsuario);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new ProgresoIMC
                {
                    FechaRegistro = reader.GetDateTime("FechaRegistro"),
                    IMC = reader.GetDouble("IMC")
                });
            }

            return lista;
        }
    }
}
