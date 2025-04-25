using BodyBalanceNow.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace BodyBalanceNow.Services
{
    public class ExerciseDatabaseAndroid
    {
        private string connectionString = "Server=sql7.freesqldatabase.com;Port = 3306; Database=sql7774904;User Id = sql7774904; Password=ByqfDxIdsa;";

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

        // AGUA
        // Método para guardar el registro de agua
        public async Task GuardarRegistroAgua(RegistroAgua registro)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var query = "INSERT INTO RegistroAgua (UsuarioId, Fecha, Cantidad) VALUES (@UsuarioId, @Fecha, @Cantidad)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", registro.UsuarioId);
            command.Parameters.AddWithValue("@Fecha", registro.Fecha.Date);
            command.Parameters.AddWithValue("@Cantidad", registro.Cantidad);

            await command.ExecuteNonQueryAsync();
        }

        // Método para obtener el total de agua consumida hoy
        public async Task<int> ObtenerTotalAguaHoy(int usuarioId, DateTime fecha)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var query = "SELECT IFNULL(SUM(Cantidad), 0) FROM RegistroAgua WHERE UsuarioId = @UsuarioId AND Fecha = @Fecha";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);
            command.Parameters.AddWithValue("@Fecha", fecha.Date);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Método para obtener el total de agua consumida en un rango de fechas
        public async Task<List<RegistroAgua>> ObtenerHistorialAgua(int usuarioId)
        {
            var lista = new List<RegistroAgua>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT Id, UsuarioId, Fecha, Cantidad FROM RegistroAgua WHERE UsuarioId = @UsuarioId ORDER BY Fecha DESC";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new RegistroAgua
                {
                    Id = reader.GetInt32("Id"),
                    UsuarioId = reader.GetInt32("UsuarioId"),
                    Fecha = reader.GetDateTime("Fecha"),
                    Cantidad = reader.GetInt32("Cantidad")
                });
            }

            return lista;
        }

        //  Método para obtener el total de agua consumida en un rango de fechas
        public async Task EliminarRegistroAgua(int idRegistro)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM RegistroAgua WHERE Id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", idRegistro);

            await command.ExecuteNonQueryAsync();
        }

        // Método para obtener el consumo de agua en los últimos 7 días
        public async Task<List<ConsumoDia>> ObtenerConsumoUltimos7Dias(int usuarioId)
        {
            var lista = new List<ConsumoDia>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"
                SELECT Fecha, SUM(Cantidad) AS Total
                FROM RegistroAgua
                WHERE UsuarioId = @UsuarioId AND Fecha >= CURDATE() - INTERVAL 6 DAY
                GROUP BY Fecha
                ORDER BY Fecha";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new ConsumoDia
                {
                    Fecha = reader.GetDateTime("Fecha"),
                    Total = reader.GetInt32("Total")
                });
            }

            return lista;
        }

        // ──────────────── SUEÑO ────────────────
        // GUARDAR SUEÑO
        public async Task GuardarRegistroSueno(RegistroSueno registro)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"INSERT INTO RegistroSueno 
                             (UsuarioId, Fecha, HoraDormir, HoraDespertar, HorasDormidas) 
                             VALUES (@UsuarioId, @Fecha, @HoraDormir, @HoraDespertar, @HorasDormidas)";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", registro.UsuarioId);
            command.Parameters.AddWithValue("@Fecha", registro.Fecha.Date);
            command.Parameters.AddWithValue("@HoraDormir", registro.HoraDormir.ToString(@"hh\:mm\:ss"));
            command.Parameters.AddWithValue("@HoraDespertar", registro.HoraDespertar.ToString(@"hh\:mm\:ss"));
            command.Parameters.AddWithValue("@HorasDormidas", registro.HorasDormidas);

            await command.ExecuteNonQueryAsync();
        }

        // HISTORIAL DE SUEÑO
        public async Task<List<RegistroSueno>> ObtenerHistorialSueno(int usuarioId)
        {
            var lista = new List<RegistroSueno>();
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"SELECT Id, UsuarioId, Fecha, HoraDormir, HoraDespertar, HorasDormidas 
                             FROM RegistroSueno 
                             WHERE UsuarioId = @UsuarioId 
                             ORDER BY Fecha DESC";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Manejo seguro del TimeSpan
                TimeSpan.TryParse(reader["HoraDormir"].ToString(), out var dormir);
                TimeSpan.TryParse(reader["HoraDespertar"].ToString(), out var despertar);

                lista.Add(new RegistroSueno
                {
                    Id = reader.GetInt32("Id"),
                    UsuarioId = reader.GetInt32("UsuarioId"),
                    Fecha = reader.GetDateTime("Fecha"),
                    HoraDormir = dormir,
                    HoraDespertar = despertar,
                    HorasDormidas = reader.GetDouble("HorasDormidas")
                });
            }

            return lista;
        }

        // ELIMINAR REGISTRO DE SUEÑO
        public async Task EliminarRegistroSueno(int idRegistro)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "DELETE FROM RegistroSueno WHERE Id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", idRegistro);

            await command.ExecuteNonQueryAsync();
        }

        // DATOS PARA GRÁFICO SEMANAL DE SUEÑO
        public async Task<List<ConsumoDia>> ObtenerSuenoUltimos7Dias(int usuarioId)
        {
            var lista = new List<ConsumoDia>();
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"
                SELECT Fecha, SUM(HorasDormidas) AS TotalHoras
                FROM RegistroSueno
                WHERE UsuarioId = @UsuarioId AND Fecha >= CURDATE() - INTERVAL 6 DAY
                GROUP BY Fecha
                ORDER BY Fecha";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new ConsumoDia
                {
                    Fecha = reader.GetDateTime("Fecha"),
                    Total = (int)Math.Round(reader.GetDouble("TotalHoras")) // o usa double si lo prefieres
                });
            }

            return lista;
        }

        //--------- ESTRES -----------
        public async Task GuardarRegistroEstres(RegistroEstres registro)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"INSERT INTO RegistroEstres (UsuarioId, Fecha, Nivel) 
                     VALUES (@UsuarioId, @Fecha, @Nivel)";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", registro.UsuarioId);
            command.Parameters.AddWithValue("@Fecha", registro.Fecha.Date);
            command.Parameters.AddWithValue("@Nivel", registro.Nivel);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<RegistroEstres>> ObtenerHistorialEstres(int usuarioId)
        {
            var lista = new List<RegistroEstres>();
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"SELECT Id, UsuarioId, Fecha, Nivel 
                     FROM RegistroEstres 
                     WHERE UsuarioId = @UsuarioId 
                     ORDER BY Fecha DESC";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new RegistroEstres
                {
                    Id = reader.GetInt32("Id"),
                    UsuarioId = reader.GetInt32("UsuarioId"),
                    Fecha = reader.GetDateTime("Fecha"),
                    Nivel = reader.GetInt32("Nivel")
                });
            }

            return lista;
        }

        public async Task EliminarRegistroEstres(int idRegistro)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "DELETE FROM RegistroEstres WHERE Id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", idRegistro);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<ConsumoDia>> ObtenerEstresUltimos7Dias(int usuarioId)
        {
            var lista = new List<ConsumoDia>();
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"
        SELECT Fecha, AVG(Nivel) AS Promedio
        FROM RegistroEstres
        WHERE UsuarioId = @UsuarioId AND Fecha >= CURDATE() - INTERVAL 6 DAY
        GROUP BY Fecha
        ORDER BY Fecha";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new ConsumoDia
                {
                    Fecha = reader.GetDateTime("Fecha"),
                    Total = (int)Math.Round(reader.GetDouble("Promedio"))
                });
            }

            return lista;
        }

        // ------- PESO --------
        // Guardar nuevo registro de peso
        public async Task GuardarRegistroPeso(PesoModel registro)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string query = "INSERT INTO RegistroPeso (UsuarioId, Fecha, Peso) VALUES (@UsuarioId, @Fecha, @Peso)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", registro.UsuarioId);
            command.Parameters.AddWithValue("@Fecha", registro.Fecha);
            command.Parameters.AddWithValue("@Peso", registro.Peso);
            await command.ExecuteNonQueryAsync();
        }


        // Obtener historial de peso
        public async Task<List<PesoModel>> ObtenerHistorialPeso(int usuarioId)
        {
            var lista = new List<PesoModel>();
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            string query = "SELECT * FROM RegistroPeso WHERE UsuarioId = @UsuarioId ORDER BY Fecha DESC";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new PesoModel
                {
                    ID = reader.GetInt32("Id"),
                    UsuarioId = reader.GetInt32("UsuarioId"),
                    Fecha = reader.GetDateTime("Fecha"),
                    Peso = reader.GetDecimal("Peso")
                });
            }
            return lista;
        }

        // Coge los ultimos 7 registros 
        public async Task<List<ConsumoDia2>> ObtenerPesoUltimos7Dias(int usuarioId)
        {
            var lista = new List<ConsumoDia2>();

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"
        SELECT Fecha, Peso
        FROM RegistroPeso
        WHERE UsuarioId = @UsuarioId
        ORDER BY Fecha DESC
        LIMIT 7";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@UsuarioId", usuarioId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new ConsumoDia2
                {
                    Fecha = reader.GetDateTime("Fecha").Date,
                    Total = Convert.ToDouble(reader.GetDecimal("Peso")) // ✅ sin redondear
                });
            }

            // Ordenamos de más antiguo a más reciente
            lista.Reverse();

            return lista;
        }

        // Eliminar registro de peso
        public async Task EliminarRegistroPeso(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "DELETE FROM RegistroPeso WHERE Id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            await command.ExecuteNonQueryAsync();
        }

    }
}

