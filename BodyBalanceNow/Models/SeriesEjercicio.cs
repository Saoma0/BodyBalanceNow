namespace BodyBalanceNow
{
    public class SeriesEjercicio
    {
        public int Id { get; set; }
        public int IDEjercicioEnRutina { get; set; } // Asegúrate de que este campo sea el correcto
        public int NumeroSerie { get; set; }
        public int RepeticionesRealizadas { get; set; }
        public float PesoRealizado { get; set; }

        public string NombreEjercicio { get; set; }
        public string NivelDificultad { get; set; }
        public string GrupoMuscular { get; set; }

        public int IDRutina { get; set; }
        public string NombreRutina { get; set; }
    }
}
