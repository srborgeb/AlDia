using SQLite;

namespace AlDia.Models
{
    public class Documento
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nombre { get; set; } // Ej: Pasaporte, Licencia, Cedula
        public string Numero { get; set; } // Ej: V-12345678
        public DateTime FechaVencimiento { get; set; }
        public int DiasAnticipacion { get; set; } // Días antes para avisar
        public bool NotificacionProgramada { get; set; }
    }
}