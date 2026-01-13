using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.LocalNotification;
using AlDia.Models;
using AlDia.Services;
using System.Collections.ObjectModel;

namespace AlDia.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _dbService;

        public ObservableCollection<Documento> Documentos { get; set; } = new();

        [ObservableProperty]
        string nombre;

        [ObservableProperty]
        string numero;

        [ObservableProperty]
        DateTime fechaVencimiento = DateTime.Today.AddYears(1);

        [ObservableProperty]
        int diasAnticipacion = 7;

        public MainViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            // Solución a Warning CS4014: Ejecutar en segundo plano
            Task.Run(async () => await CargarDocumentos());
        }

        [RelayCommand]
        public async Task CargarDocumentos()
        {
            var lista = await _dbService.GetDocumentosAsync();
            Documentos.Clear();
            foreach (var doc in lista)
            {
                Documentos.Add(doc);
            }
        }

        [RelayCommand]
        public async Task Guardar()
        {
            if (string.IsNullOrWhiteSpace(Nombre)) return;

            var nuevoDoc = new Documento
            {
                Nombre = Nombre,
                Numero = Numero,
                FechaVencimiento = FechaVencimiento,
                DiasAnticipacion = DiasAnticipacion
            };

            // 1. Guardar en Base de Datos
            await _dbService.SaveDocumentoAsync(nuevoDoc);

            // 2. Programar Notificación
            ProgramarNotificacion(nuevoDoc);

            // 3. Limpiar UI y recargar
            Nombre = string.Empty;
            Numero = string.Empty;
            await CargarDocumentos();
        }

        [RelayCommand]
        public async Task Eliminar(Documento doc)
        {
            if (doc == null) return;

            // Cancelar notificación si existe
            LocalNotificationCenter.Current.Cancel(doc.Id);

            await _dbService.DeleteDocumentoAsync(doc);
            await CargarDocumentos();
        }

        private void ProgramarNotificacion(Documento doc)
        {
            var fechaNotificacion = doc.FechaVencimiento.AddDays(-doc.DiasAnticipacion);

            // Si la fecha de aviso ya pasó, no programamos (o avisamos hoy)
            if (fechaNotificacion < DateTime.Now) return;

            var notification = new NotificationRequest
            {
                NotificationId = doc.Id, // Usamos el ID del documento para identificar la alerta
                Title = "¡Documento por vencer!",
                Description = $"Tu documento '{doc.Nombre}' vence el {doc.FechaVencimiento:dd/MM/yyyy}.",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = fechaNotificacion,
                    RepeatType = NotificationRepeat.No // Solo una vez
                }
            };

            LocalNotificationCenter.Current.Show(notification);
        }
    }
}