using SQLite;

namespace AlDia.Models
{
    public class Documento
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Nueva propiedad para rastrear cuándo debe sonar la alarma
        public DateTime FechaProximoRecordatorio { get; set; }

        public bool EstaAlDia { get; set; }

        public byte[] DatosFoto { get; set; }

        public int DiasRepeticion { get; set; }

        [Ignore]
        public bool TieneFoto => DatosFoto != null && DatosFoto.Length > 0;

        [Ignore]
        public string TextoRepeticion => DiasRepeticion > 0 ? $"Repetir cada {DiasRepeticion} días" : "Sin repetición";
    }
}