using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;

namespace BodyBalanceNow.Services
{
    public class DatabaseServiceAndroid
    {
        private string connectionString = "Server=sql7.freesqldatabase.com;Port = 3306; Database=sql7774904;User Id = sql7774904; Password=ByqfDxIdsa;";



        public List<Ejercicios> GetEjerciciosPorPagina(int pagina, int tamañoPagina)
        {
            var ejercicios = new List<Ejercicios>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                int offset = (pagina - 1) * tamañoPagina;

                string query = @"SELECT ID, NombreEjercicio, UrlDemostracionCorta, UrlDemostracionLarga, 
                                NivelDificultad, GrupoMuscular, Equipamiento, Agarre, 
                                ZonaCorporal, Lateralidad 
                         FROM ListaEjercicios 
                         LIMIT @tamañoPagina OFFSET @offset";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@tamañoPagina", tamañoPagina);
                    command.Parameters.AddWithValue("@offset", offset);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ejercicio = new Ejercicios
                            {
                                ID = reader.GetInt32("ID"),
                                NombreEjercicio = reader.GetString("NombreEjercicio"),
                                UrlDemostracionCorta = reader.IsDBNull(reader.GetOrdinal("UrlDemostracionCorta")) ? string.Empty : reader.GetString("UrlDemostracionCorta"),
                                UrlDemostracionLarga = reader.IsDBNull(reader.GetOrdinal("UrlDemostracionLarga")) ? string.Empty : reader.GetString("UrlDemostracionLarga"),
                                NivelDificultad = reader.GetString("NivelDificultad"),
                                GrupoMuscular = reader.GetString("GrupoMuscular"),
                                Equipamiento = reader.GetString("Equipamiento"),
                                Agarre = reader.GetString("Agarre"),
                                ZonaCorporal = reader.GetString("ZonaCorporal"),
                                Lateralidad = reader.GetString("Lateralidad")
                            };

                            ejercicios.Add(ejercicio);
                        }
                    }
                }
            }

            return ejercicios;
        }





        public int GetCantidadDatos()
        {
            int totalRegistros = 0;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Consulta para contar el total de registros en la tabla ListaEjercicios
                string query = "SELECT COUNT(*) FROM ListaEjercicios";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    totalRegistros = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return totalRegistros;
        }


        public int InsertarRutina(string nombreRutina, int idUsuario)
        {
            int idRutina = 0; // Variable para almacenar el ID generado

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Rutinas (NombreRutina, IDUsuario) VALUES (@NombreRutina, @IDUsuario); SELECT LAST_INSERT_ID();";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NombreRutina", nombreRutina);
                    command.Parameters.AddWithValue("@IDUsuario", idUsuario);

                    // Ejecuta la consulta y obtiene el ID de la rutina recién insertada
                    idRutina = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return idRutina; // Devuelve el ID de la rutina
        }

        public void InsertarEjercicioEnRutina(int idRutina, Ejercicios ejercicio)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO EjerciciosEnRutina (IDRutina, IDEjercicio) VALUES (@IDRutina, @IDEjercicio)";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IDRutina", idRutina);
                    command.Parameters.AddWithValue("@IDEjercicio", ejercicio.ID); // Asegúrate de que Ejercicios tenga una propiedad ID

                    command.ExecuteNonQuery(); // Ejecuta el INSERT
                }
            }
        }
        public List<Rutina> GetRutinasPorUsuario(int idUsuario)
        {
            List<Rutina> rutinas = new List<Rutina>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT ID, NombreRutina, FechaCreacion FROM Rutinas WHERE IDUsuario = @IDUsuario";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IDUsuario", idUsuario);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Rutina rutina = new Rutina();

                            rutina.ID = reader.IsDBNull(reader.GetOrdinal("ID")) ? 0 : reader.GetInt32("ID");
                            rutina.NombreRutina = reader.IsDBNull(reader.GetOrdinal("NombreRutina")) ? string.Empty : reader.GetString("NombreRutina");
                            rutina.FechaCreacion = reader.IsDBNull(reader.GetOrdinal("FechaCreacion")) ? DateTime.MinValue : reader.GetDateTime("FechaCreacion");

                            rutinas.Add(rutina);
                        }
                    }
                }
            }

            return rutinas;
        }


        public List<EjerciciosRutina> GetEjerciciosPorRutina(int idRutina)
        {
            List<EjerciciosRutina> ejercicios = new List<EjerciciosRutina>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
            SELECT 
                e.ID,
                e.NombreEjercicio,
                e.NivelDificultad,
                e.GrupoMuscular,
                e.UrlDemostracionCorta,
                e.UrlDemostracionLarga
            FROM EjerciciosEnRutina er
            JOIN ListaEjercicios e ON er.IDEjercicio = e.ID
            WHERE er.IDRutina = @IDRutina";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IDRutina", idRutina);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EjerciciosRutina ejercicio = new EjerciciosRutina
                            {
                                ID = reader.GetInt32("ID"),
                                NombreEjercicio = reader.GetString("NombreEjercicio"),
                                Dificultad = reader.GetString("NivelDificultad"),
                                GrupoMuscular = reader.GetString("GrupoMuscular"),
                                urlCorta = reader.IsDBNull(reader.GetOrdinal("UrlDemostracionCorta")) ? "" : reader.GetString("UrlDemostracionCorta"),
                                urlLarga = reader.IsDBNull(reader.GetOrdinal("UrlDemostracionLarga")) ? "" : reader.GetString("UrlDemostracionLarga")
                            };

                            ejercicios.Add(ejercicio);
                        }
                    }
                }
            }

            return ejercicios;
        }
        public  SeriesEjercicio ObtenerDetallesRutinaYEjercicio(int idRutina, int idEjercicio)
        {
            SeriesEjercicio detalle = null;

            try
            {
                using (var connection = new MySqlConnection(connectionString)) // O SqlConnection si es SQL Server
                {
                    connection.Open();
                    var query = @"
                    SELECT 
                        r.ID AS IDRutina,
                        r.NombreRutina,
                        e.ID AS IDEjercicio,
                        e.NombreEjercicio,
                        e.NivelDificultad,
                        e.GrupoMuscular
                    FROM Rutinas r
                    JOIN EjerciciosEnRutina eer ON r.ID = eer.IDRutina
                    JOIN ListaEjercicios e ON eer.IDEjercicio = e.ID
                    WHERE r.ID = @idRutina AND e.ID = @idEjercicio";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idRutina", idRutina);
                        command.Parameters.AddWithValue("@idEjercicio", idEjercicio);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                detalle = new SeriesEjercicio
                                {
                                    IDRutina = reader.GetInt32("IDRutina"),
                                    NombreRutina = reader.GetString("NombreRutina"),
                                    IDEjercicioEnRutina = reader.GetInt32("IDEjercicio"), // Asegúrate de que este campo se corresponda con la base de datos
                                    NombreEjercicio = reader.GetString("NombreEjercicio"),
                                    NivelDificultad = reader.GetString("NivelDificultad"),
                                    GrupoMuscular = reader.GetString("GrupoMuscular")
                                };
                            }
                            else
                            {
                                Debug.WriteLine($"No se encontraron resultados para Rutina: {idRutina}, Ejercicio: {idEjercicio}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ObtenerDetallesRutinaYEjercicio: {ex.Message}");
            }

            return detalle;
        }


        public async Task InsertarSerieRealizada(int idEjercicioEnRutina, int numeroSerie, int repeticionesRealizadas, float pesoRealizado)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    // Abre la conexión a la base de datos
                    await connection.OpenAsync();

                    // Consulta SQL para insertar datos
                    var query = @"
                            INSERT INTO SeriesRealizadas (IDEjercicioEnRutina, NumeroSerie, RepeticionesRealizadas, PesoRealizado)
                            VALUES (@IDEjercicioEnRutina, @NumeroSerie, @RepeticionesRealizadas, @PesoRealizado)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        // Asignación de parámetros para evitar inyecciones SQL
                        command.Parameters.AddWithValue("@IDEjercicioEnRutina", idEjercicioEnRutina);
                        command.Parameters.AddWithValue("@NumeroSerie", numeroSerie);
                        command.Parameters.AddWithValue("@RepeticionesRealizadas", repeticionesRealizadas);
                        command.Parameters.AddWithValue("@PesoRealizado", pesoRealizado);

                        // Ejecuta la consulta SQL
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        Debug.WriteLine($"Filas afectadas: {rowsAffected}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                Debug.WriteLine($"Error general: {ex.Message}");
            }
        }

        public async Task<int> ObtenerIdEjercicioEnRutinaAsync(int idRutina, int idEjercicio)
        {
            int idEjercicioEnRutina = 0;

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                            SELECT ID 
                            FROM EjerciciosEnRutina 
                            WHERE IDRutina = @IDRutina AND IDEjercicio = @IDEjercicio";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        // Asignar los parámetros para la consulta
                        command.Parameters.AddWithValue("@IDRutina", idRutina);
                        command.Parameters.AddWithValue("@IDEjercicio", idEjercicio);

                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            idEjercicioEnRutina = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general: {ex.Message}");
                
            }

            return idEjercicioEnRutina;
        }


        public async Task<List<SeriesEjercicio>> ObtenerSeriesPorEjercicioAsync(int idEjercicioEnRutina)
        {
            var series = new List<SeriesEjercicio>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                                SELECT NumeroSerie, RepeticionesRealizadas, PesoRealizado 
                                FROM SeriesRealizadas 
                                WHERE IDEjercicioEnRutina = @IDEjercicioEnRutina";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IDEjercicioEnRutina", idEjercicioEnRutina);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var serie = new SeriesEjercicio
                                {
                                    NumeroSerie = reader.GetInt32("NumeroSerie"),
                                    RepeticionesRealizadas = reader.GetInt32("RepeticionesRealizadas"),
                                    PesoRealizado = reader.GetFloat("PesoRealizado")
                                };
                                series.Add(serie);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error general: {ex.Message}");
            }

            return series;
        }


        public async Task EliminarEjercicioEnRutinaAsync(int idEjercicioEnRutina)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = "DELETE FROM EjerciciosEnRutina WHERE ID = @ID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", idEjercicioEnRutina);

                        // Ejecutar el DELETE
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al eliminar el ejercicio: {ex.Message}");
               
            }
        }

        public async Task EliminarRutinaAsync(int idRutina)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = "DELETE FROM Rutinas WHERE ID = @ID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", idRutina);

                        // Ejecutar el DELETE
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al eliminar la rutina: {ex.Message}");
            }
        }
        public async Task EditarNombreRutinaAsync(int idRutina, string nuevoNombre)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = "UPDATE Rutinas SET NombreRutina = @NombreRutina WHERE ID = @ID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NombreRutina", nuevoNombre);
                        command.Parameters.AddWithValue("@ID", idRutina);

                        // Ejecutar la actualización
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al editar el nombre de la rutina: {ex.Message}");
            }
        }
        public async Task<Ejercicios> ObtenerEjercicioPorIdAsync(int idEjercicio)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT * FROM ListaEjercicios WHERE ID = @ID";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", idEjercicio);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Ejercicios
                                {
                                    ID = reader.GetInt32("ID"),
                                    NombreEjercicio = reader.GetString("NombreEjercicio"),
                                    UrlDemostracionCorta = reader.GetString("UrlDemostracionCorta"),
                                    UrlDemostracionLarga = reader.GetString("UrlDemostracionLarga"),
                                    NivelDificultad = reader.GetString("NivelDificultad"),
                                    GrupoMuscular = reader.GetString("GrupoMuscular"),
                                    Equipamiento = reader.GetString("Equipamiento"),
                                    Agarre = reader.GetString("Agarre"),
                                    ZonaCorporal = reader.GetString("ZonaCorporal"),
                                    Lateralidad = reader.GetString("Lateralidad")
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener el ejercicio: {ex.Message}");
            }

            return null; // Si no se encuentra el ejercicio
        }



    }
}

