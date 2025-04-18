using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyBalanceNow.Models
{
    public class PesoModel
    {
        public int ID { get; set; }
        public int UsuarioId { get; set; } // renombrado
        public DateTime Fecha { get; set; }
        public decimal Peso { get; set; }
    }

    public class ConsumoDia2
    {
        public DateTime Fecha { get; set; }
        public double Total { get; set; } // ✅ Corrección
    }
}
