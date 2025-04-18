using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyBalanceNow
{
    public class Ejercicios
    {
        public int ID { get; set; }
        public string NombreEjercicio { get; set; }
        public string UrlDemostracionCorta { get; set; }
        public string UrlDemostracionLarga { get; set; }
        public string NivelDificultad { get; set; }
        public string GrupoMuscular { get; set; }
        public string Equipamiento { get; set; }
        public string Agarre { get; set; }
        public string ZonaCorporal { get; set; }
        public string Lateralidad { get; set; }
    }

}
