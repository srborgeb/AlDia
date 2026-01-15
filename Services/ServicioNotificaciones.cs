using AlDia.Models;
using System;

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
            // Cambiado 'Titulo' por 'Nombre' y asegurando el uso de 'FechaProximoRecordatorio'
            if (documento.DiasRepeticion <= 0 && documento.FechaProximoRecordatorio == DateTime.MinValue) return;

            var fechaNotificacion = documento.FechaProximoRecordatorio;

            if (fechaNotificacion < DateTime.Now)
                return;

            // Simulación de despacho al sistema
            System.Diagnostics.Debug.WriteLine($"[SISTEMA] Notificación programada: {documento.Nombre} para el {fechaNotificacion}");
        }

        public void CancelarNotificacion(int documentoId)
        {
            System.Diagnostics.Debug.WriteLine($"[SISTEMA] Notificación {documentoId} cancelada");
        }
    }
}