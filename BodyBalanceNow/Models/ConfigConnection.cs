using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BodyBalanceNow.Models
{
    public class ConfigConnection
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string NameUser { get; set; }
        public string PasswordUser { get; set; }

        
        // Ruta del arhcivo .json
        private static readonly string rutaArchivo = "C:\\Users\\samue\\Desktop\\TFG\\BodyBalanceNow\\BodyBalanceNow\\BodyBalanceNow\\Resources\\Json\\ConfigConnection.json";

        // Método para cargar la configuración desde el archivo JSON <Deserializar>
        public static ConfigConnection CargarDesdeJson()
        {
            string contenidoJson = File.ReadAllText(rutaArchivo);
            return JsonSerializer.Deserialize<ConfigConnection>(contenidoJson);
        }

        // Método para obtener el string de conexión
        public string GetConnectionString()
        {
            return $"Server={Server};Database={Database};User Id={NameUser};Password={PasswordUser};";
        }
    }

     
}