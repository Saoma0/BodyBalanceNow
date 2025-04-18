using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyBalanceNow.Models
{
    public class RegistroEstres
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public int Nivel { get; set; } // Nivel de estrés de 0 a 10
    }
}
