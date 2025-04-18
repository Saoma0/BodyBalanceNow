using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyBalanceNow.Models
{
    public class RegistroSueno
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraDormir { get; set; }
        public TimeSpan HoraDespertar { get; set; }
        public double HorasDormidas { get; set; }
    }
}
