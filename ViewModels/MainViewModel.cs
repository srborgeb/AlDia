using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using AlDia.Models;
using AlDia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// Especificamos el uso de Graphics para evitar ambigüedades
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Media;

namespace AlDia.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ServicioBaseDatos _servicioBaseDatos;
        private readonly IServicioNotificaciones _servicioNotificaciones;

        [ObservableProperty]
        private ObservableCollection<Documento> documentos;

        [ObservableProperty]
        private string nuevoTitulo;

        [ObservableProperty]
        private string nuevaDescripcion;

        [ObservableProperty]
        private string diasRepeticionTexto = "0";

        [ObservableProperty]
        private byte[] datosFotoTemporal;

        [ObservableProperty]
        private Documento documentoSeleccionado;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextoBotonGuardar))]
        private bool estaEditando;

        public string TextoBotonGuardar => EstaEditando ? "ACTUALIZAR DOCUMENTO" : "GUARDAR REGISTRO";

        public MainViewModel(ServicioBaseDatos servicioBaseDatos, IServicioNotificaciones servicioNotificaciones)
        {
            _servicioBaseDatos = servicioBaseDatos;
            _servicioNotificaciones = servicioNotificaciones;
            Documentos = new ObservableCollection<Documento>();
            _ = CargarDocumentos();
        }

        [RelayCommand]
        public void PrepararEdicion(Documento documento)
        {
            if (documento == null) return;
            DocumentoSeleccionado = documento;
            NuevoTitulo = documento.Titulo;
            NuevaDescripcion = documento.Descripcion;
            DiasRepeticionTexto = documento.DiasRepeticion.ToString();
            DatosFotoTemporal = documento.DatosFoto;
            EstaEditando = true;
        }

        [RelayCommand]
        public void CancelarEdicion() => LimpiarFormulario();

        [RelayCommand]
        public async Task SeleccionarFoto()
        {
            try
            {
                var foto = await MediaPicker.Default.PickPhotoAsync();
                await ProcesarFotoAsync(foto);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        [RelayCommand]
        public async Task TomarFoto()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var foto = await MediaPicker.Default.CapturePhotoAsync();
                    await ProcesarFotoAsync(foto);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private async Task ProcesarFotoAsync(FileResult foto)
        {
            if (foto != null)
            {
                using var flujo = await foto.OpenReadAsync();
                DatosFotoTemporal = await ComprimirImagenAsync(flujo);
            }
        }

        private async Task<byte[]> ComprimirImagenAsync(Stream flujo)
        {
            // Usamos el nombre completo para evitar la ambigüedad de IImage
            Microsoft.Maui.Graphics.IImage imagen = PlatformImage.FromStream(flujo);
            if (imagen == null) return null;

            float dimensionMaxima = 1024;
            float ancho = imagen.Width;
            float alto = imagen.Height;

            if (ancho > dimensionMaxima || alto > dimensionMaxima)
            {
                if (ancho > alto)
                {
                    alto = (dimensionMaxima / ancho) * alto;
                    ancho = dimensionMaxima;
                }
                else
                {
                    ancho = (dimensionMaxima / alto) * ancho;
                    alto = dimensionMaxima;
                }
            }

            var imagenRedimensionada = imagen.Downsize(ancho, alto, true);

            using var ms = new MemoryStream();
            // El método SaveAsync ahora se reconocerá correctamente al resolver la ambigüedad de la imagen
            await imagenRedimensionada.SaveAsync(ms, ImageFormat.Jpeg, 0.75f);
            return ms.ToArray();
        }

        [RelayCommand]
        public async Task CargarDocumentos()
        {
            var items = await _servicioBaseDatos.ObtenerDocumentosAsync();
            Documentos.Clear();
            foreach (var item in items) Documentos.Add(item);
        }

        [RelayCommand]
        public async Task GuardarDocumento()
        {
            if (string.IsNullOrWhiteSpace(NuevoTitulo)) return;
            int.TryParse(DiasRepeticionTexto, out int dias);

            if (EstaEditando && DocumentoSeleccionado != null)
            {
                DocumentoSeleccionado.Titulo = NuevoTitulo;
                DocumentoSeleccionado.Descripcion = NuevaDescripcion;
                DocumentoSeleccionado.DiasRepeticion = dias;
                DocumentoSeleccionado.DatosFoto = DatosFotoTemporal;
                DocumentoSeleccionado.FechaProximoRecordatorio = DateTime.Now.AddDays(dias > 0 ? dias : 1);

                await _servicioBaseDatos.GuardarDocumentoAsync(DocumentoSeleccionado);
                _servicioNotificaciones.ProgramarNotificacion(DocumentoSeleccionado);
            }
            else
            {
                var nuevo = new Documento
                {
                    Titulo = NuevoTitulo,
                    Descripcion = NuevaDescripcion,
                    FechaCreacion = DateTime.Now,
                    DatosFoto = DatosFotoTemporal,
                    DiasRepeticion = dias,
                    FechaProximoRecordatorio = DateTime.Now.AddDays(dias > 0 ? dias : 1)
                };
                await _servicioBaseDatos.GuardarDocumentoAsync(nuevo);
                if (nuevo.DiasRepeticion > 0) _servicioNotificaciones.ProgramarNotificacion(nuevo);
            }

            LimpiarFormulario();
            await CargarDocumentos();
        }

        private void LimpiarFormulario()
        {
            NuevoTitulo = string.Empty;
            NuevaDescripcion = string.Empty;
            DiasRepeticionTexto = "0";
            DatosFotoTemporal = null;
            DocumentoSeleccionado = null;
            EstaEditando = false;
        }

        [RelayCommand]
        public async Task EliminarDocumento(Documento documento)
        {
            if (documento == null) return;
            _servicioNotificaciones.CancelarNotificacion(documento.Id);
            await _servicioBaseDatos.EliminarDocumentoAsync(documento);
            await CargarDocumentos();
        }
    }
}