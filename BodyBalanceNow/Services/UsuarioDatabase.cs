using BCrypt;
using BodyBalanceNow.Models;
using MySql.Data.MySqlClient;
namespace BodyBalanceNow.Services
{
    public  class UsuarioDatabase
    {

         private string connectionString = "Server=localhost;Database=tfgdatabase;User Id = root; Password=S@6493483!!!!;";
        public void RegistrarUsuario(Usuario usuario)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Usuarios (Nombre, Email, PasswordUser) " +
                                   "VALUES (@Nombre, @Email, @PasswordUser)";

                    // Hashear la contraseña antes de guardarla.
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuario.Password);

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", usuario.Name);
                        command.Parameters.AddWithValue("@Email", usuario.Email);

                        command.Parameters.AddWithValue("@PasswordUser", hashedPassword);


                        command.ExecuteNonQuery(); // Guarda el usuario en la base de datos.
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar el usuario: {ex.Message}");
            }
        }

        public Usuario Login(string email, string password)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Usuarios WHERE Email = @Email";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Obtén la contraseña hasheada de la base de datos.
                                string storedHashedPassword = reader["PasswordUser"].ToString();

                                // Compara la contraseña ingresada con la almacenada usando BCrypt.Net.
                                if (BCrypt.Net.BCrypt.Verify(password, storedHashedPassword))
                                {
                                    // Credenciales válidas, devuelve el objeto User.
                                    return new Usuario
                                    {
                                        Id = Convert.ToInt32(reader["ID"]),
                                        Name = reader["Nombre"].ToString(),
                                        Email = reader["Email"].ToString(),
                                        Password = storedHashedPassword
                                        
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar sesión: {ex.Message}");
            }

            return null; // Si las credenciales no son válidas o ocurre un error.
        }

        public bool UsuarioExiste(string email)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        var count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0; // Devuelve true si hay al menos un usuario con ese correo.
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al verificar el usuario: {ex.Message}");
                return false; // Devuelve false en caso de error.
            }
        }
    }
}
