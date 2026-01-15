using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using AlDia.Models;
using AlDia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Media;
using Microsoft.Maui.ApplicationModel;

namespace AlDia.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ServicioBaseDatos _servicioBaseDatos;
        private readonly IServicioNotificaciones _servicioNotificaciones;

        // Propiedades de la Pantalla Principal
        [ObservableProperty] private string versionApp;
        [ObservableProperty] private string estadoSincronizacion = "Base de datos local activa";
        [ObservableProperty] private double progresoSincronizacion = 1.0;

        // Propiedades del Formulario
        [ObservableProperty] private ObservableCollection<Documento> documentos;
        [ObservableProperty] private string nombreDoc;
        [ObservableProperty] private string numeroDoc;
        [ObservableProperty] private string descripcionDoc;
        [ObservableProperty] private DateTime fechaVencimiento = DateTime.Now;
        [ObservableProperty] private int diasAnticipacion = 5;
        [ObservableProperty] private int diasRepeticion = 1;
        [ObservableProperty] private byte[] datosFotoTemporal;

        [ObservableProperty] private Documento documentoSeleccionado;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoBotonGuardar))]
        private bool estaEditando;

        public string TextoBotonGuardar => EstaEditando ? "ACTUALIZAR" : "GUARDAR DOCUMENTO";

        public MainViewModel(ServicioBaseDatos servicioBaseDatos, IServicioNotificaciones servicioNotificaciones)
        {
            _servicioBaseDatos = servicioBaseDatos;
            _servicioNotificaciones = servicioNotificaciones;
            Documentos = new ObservableCollection<Documento>();

            // Cargar versión dinámica
            VersionApp = $"Versión: {AppInfo.VersionString} (Build {AppInfo.BuildString})";

            _ = CargarDocumentos();
        }

        [RelayCommand]
        public async Task TomarFoto()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var foto = await MediaPicker.Default.CapturePhotoAsync();
                    if (foto != null)
                    {
                        using var stream = await foto.OpenReadAsync();
                        DatosFotoTemporal = await ComprimirImagenAsync(stream);
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        [RelayCommand]
        public async Task SeleccionarFoto()
        {
            try
            {
                var foto = await MediaPicker.Default.PickPhotoAsync();
                if (foto != null)
                {
                    using var stream = await foto.OpenReadAsync();
                    DatosFotoTemporal = await ComprimirImagenAsync(stream);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private async Task<byte[]> ComprimirImagenAsync(Stream flujo)
        {
            Microsoft.Maui.Graphics.IImage imagen = PlatformImage.FromStream(flujo);
            if (imagen == null) return null;
            var redimensionada = imagen.Downsize(1024, 1024, true);
            using var ms = new MemoryStream();
            await redimensionada.SaveAsync(ms, ImageFormat.Jpeg, 0.75f);
            return ms.ToArray();
        }

        [RelayCommand]
        public async Task GuardarDocumento()
        {
            if (string.IsNullOrWhiteSpace(NombreDoc)) return;

            if (EstaEditando && DocumentoSeleccionado != null)
            {
                DocumentoSeleccionado.Nombre = NombreDoc;
                DocumentoSeleccionado.NumeroDocumento = NumeroDoc;
                DocumentoSeleccionado.Descripcion = DescripcionDoc;
                DocumentoSeleccionado.FechaVencimiento = FechaVencimiento;
                DocumentoSeleccionado.DiasAnticipacion = DiasAnticipacion;
                DocumentoSeleccionado.DiasRepeticion = DiasRepeticion;
                DocumentoSeleccionado.DatosFoto = DatosFotoTemporal;
                DocumentoSeleccionado.FechaProximoRecordatorio = FechaVencimiento.AddDays(-DiasAnticipacion);

                await _servicioBaseDatos.GuardarDocumentoAsync(DocumentoSeleccionado);
            }
            else
            {
                var nuevo = new Documento
                {
                    Nombre = NombreDoc,
                    NumeroDocumento = NumeroDoc,
                    Descripcion = DescripcionDoc,
                    FechaVencimiento = FechaVencimiento,
                    DiasAnticipacion = DiasAnticipacion,
                    DiasRepeticion = DiasRepeticion,
                    DatosFoto = DatosFotoTemporal,
                    FechaProximoRecordatorio = FechaVencimiento.AddDays(-DiasAnticipacion)
                };
                await _servicioBaseDatos.GuardarDocumentoAsync(nuevo);
            }

            LimpiarFormulario();
            await CargarDocumentos();
        }

        [RelayCommand]
        public void PrepararEdicion(Documento doc)
        {
            DocumentoSeleccionado = doc;
            NombreDoc = doc.Nombre;
            NumeroDoc = doc.NumeroDocumento;
            DescripcionDoc = doc.Descripcion;
            FechaVencimiento = doc.FechaVencimiento;
            DiasAnticipacion = doc.DiasAnticipacion;
            DiasRepeticion = doc.DiasRepeticion;
            DatosFotoTemporal = doc.DatosFoto;
            EstaEditando = true;
        }

        [RelayCommand]
        public void CancelarEdicion() => LimpiarFormulario();

        [RelayCommand]
        public async Task EliminarDocumento(Documento doc)
        {
            if (doc == null) return;
            await _servicioBaseDatos.EliminarDocumentoAsync(doc);
            await CargarDocumentos();
        }

        private void LimpiarFormulario()
        {
            NombreDoc = string.Empty;
            NumeroDoc = string.Empty;
            DescripcionDoc = string.Empty;
            FechaVencimiento = DateTime.Now;
            DiasAnticipacion = 5;
            DiasRepeticion = 1;
            DatosFotoTemporal = null;
            DocumentoSeleccionado = null;
            EstaEditando = false;
        }

        [RelayCommand]
        public async Task CargarDocumentos()
        {
            var items = await _servicioBaseDatos.ObtenerDocumentosAsync();
            Documentos.Clear();
            foreach (var item in items) Documentos.Add(item);
        }

        [RelayCommand]
        public void Salir() => Application.Current?.Quit();
    }
}