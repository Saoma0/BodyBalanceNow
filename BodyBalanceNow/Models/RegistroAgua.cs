using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyBalanceNow.Models
{
    public class RegistroAgua
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }

        // Opcional: incluir una propiedad de navegación
        public Usuario Usuario { get; set; }
    }

    public class ConsumoDia
    {
        public DateTime Fecha { get; set; }
        public int Total { get; set; }
    }

}
