using AlDia.Models;

namespace AlDia.Services
{
    public interface IServicioNotificaciones
    {
        void ProgramarNotificacion(Documento documento);
        void CancelarNotificacion(int documentoId);
    }

    public class ServicioNotificaciones : IServicioNotificaciones
    {
        public void ProgramarNotificacion(Documento documento)
        {
            if (documento.DiasRepeticion <= 0 && documento.FechaProximoRecordatorio == DateTime.MinValue) return;

            var fechaNotificacion = documento.FechaProximoRecordatorio;

            if (fechaNotificacion < DateTime.Now)
                return;

            // Simulación de programación de alarma nativa
            Console.WriteLine($"[SISTEMA] Notificación programada: {documento.Titulo} para el {fechaNotificacion}");
        }

        public void CancelarNotificacion(int documentoId)
        {
            Console.WriteLine($"[SISTEMA] Notificación {documentoId} cancelada");
        }
    }
}