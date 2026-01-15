using SQLite;
using System;

namespace AlDia.Models
{
    public class Documento
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string NumeroDocumento { get; set; }

        public string Descripcion { get; set; }

        public DateTime FechaVencimiento { get; set; } = DateTime.Now;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedad para la lógica de notificaciones
        public DateTime FechaProximoRecordatorio { get; set; }

        public int DiasAnticipacion { get; set; }

        public int DiasRepeticion { get; set; }

        public byte[] DatosFoto { get; set; }

        public bool EstaAlDia { get; set; }

        [Ignore]
        public bool TieneFoto => DatosFoto != null && DatosFoto.Length > 0;

        [Ignore]
        public string TextoRepeticion => DiasRepeticion > 0 ? $"Repetir cada {DiasRepeticion} días" : "Sin repetición";
    }
}