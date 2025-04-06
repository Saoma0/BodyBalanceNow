using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyBalanceNow.Models
{
    public class ProgresoIMC
    {
        public int ID { get; set; }
        public DateTime FechaRegistro { get; set; }
        public double IMC { get; set; }
        public int IdUser { get; set; }
    }
}
