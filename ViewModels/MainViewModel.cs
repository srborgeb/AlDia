using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AlDia.Models;
using AlDia.Services;

namespace AlDia.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private byte[]? _fotoActualBytes;
        private Documento? _documentoEnEdicion; // Para saber qué ID actualizar

        // --- PROPIEDADES ---
        private string nombreDoc = string.Empty;
        public string NombreDoc
        {
            get => nombreDoc;
            set { nombreDoc = value; OnPropertyChanged(); }
        }

        private string numeroDoc = string.Empty;
        public string NumeroDoc
        {
            get => numeroDoc;
            set { numeroDoc = value; OnPropertyChanged(); }
        }

        private DateTime fechaVencimiento = DateTime.Today;
        public DateTime FechaVencimiento
        {
            get => fechaVencimiento;
            set { fechaVencimiento = value; OnPropertyChanged(); }
        }

        private string diasAnticipacion = "1";
        public string DiasAnticipacion
        {
            get => diasAnticipacion;
            set { diasAnticipacion = value; OnPropertyChanged(); }
        }

        private bool estaEditando = false;
        public bool EstaEditando
        {
            get => estaEditando;
            set
            {
                estaEditando = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextoBotonGuardar));
            }
        }

        public string TextoBotonGuardar => EstaEditando ? "Actualizar Documento" : "Guardar Documento";

        public ObservableCollection<Documento> Documentos { get; set; } = new ObservableCollection<Documento>();

        // --- COMANDOS ---
        public ICommand TomarFotoCommand { get; }
        public ICommand SeleccionarFotoCommand { get; }
        public ICommand GuardarDocumentoCommand { get; }
        public ICommand CancelarEdicionCommand { get; }
        public ICommand PrepararEdicionCommand { get; }
        public ICommand EliminarDocumentoCommand { get; }
        public ICommand VerImagenCommand { get; }
        public ICommand CargarDocumentosCommand { get; }

        // Inyección de dependencias en el constructor
        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            TomarFotoCommand = new Command(async () => await TomarFoto());
            SeleccionarFotoCommand = new Command(async () => await SeleccionarFoto());
            GuardarDocumentoCommand = new Command(async () => await GuardarDocumento());
            CancelarEdicionCommand = new Command(CancelarEdicion);
            PrepararEdicionCommand = new Command<Documento>(PrepararEdicion);
            EliminarDocumentoCommand = new Command<Documento>(async (doc) => await EliminarDocumento(doc));
            VerImagenCommand = new Command<Documento>(async (doc) => await VerImagen(doc));
            CargarDocumentosCommand = new Command(async () => await CargarDocumentos());

            // Cargar datos al iniciar
            Task.Run(async () => await CargarDocumentos());
        }

        private async Task CargarDocumentos()
        {
            var docs = await _databaseService.ObtenerDocumentosAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Documentos.Clear();
                foreach (var doc in docs)
                {
                    Documentos.Add(doc);
                }
            });
        }

        private async Task GuardarDocumento()
        {
            if (string.IsNullOrWhiteSpace(NombreDoc))
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "El nombre es obligatorio", "OK");
                return;
            }

            int dias = int.TryParse(DiasAnticipacion, out int d) ? d : 1;

            if (EstaEditando && _documentoEnEdicion != null)
            {
                // Actualizar existente
                _documentoEnEdicion.Nombre = NombreDoc;
                _documentoEnEdicion.NumeroDocumento = NumeroDoc;
                _documentoEnEdicion.FechaVencimiento = FechaVencimiento;
                _documentoEnEdicion.DiasAnticipacion = dias;

                // Solo actualizamos la foto si el usuario tomó una nueva, si no, mantenemos la anterior
                if (_fotoActualBytes != null)
                {
                    _documentoEnEdicion.DatosFoto = _fotoActualBytes;
                }

                await _databaseService.GuardarDocumentoAsync(_documentoEnEdicion);
            }
            else
            {
                // Crear nuevo
                var nuevoDoc = new Documento
                {
                    Nombre = NombreDoc,
                    NumeroDocumento = NumeroDoc,
                    FechaVencimiento = FechaVencimiento,
                    DiasAnticipacion = dias,
                    DatosFoto = _fotoActualBytes
                };

                await _databaseService.GuardarDocumentoAsync(nuevoDoc);
            }

            CancelarEdicion();
            await CargarDocumentos(); // Recargar lista desde BD
        }

        private void PrepararEdicion(Documento doc)
        {
            _documentoEnEdicion = doc;
            EstaEditando = true;
            NombreDoc = doc.Nombre;
            NumeroDoc = doc.NumeroDocumento;
            FechaVencimiento = doc.FechaVencimiento;
            DiasAnticipacion = doc.DiasAnticipacion.ToString();
            _fotoActualBytes = null; // Reiniciar temporal, pero mantenemos la foto original en _documentoEnEdicion
        }

        private async Task EliminarDocumento(Documento doc)
        {
            bool confirmar = await Application.Current!.MainPage!.DisplayAlert("Eliminar", $"¿Deseas eliminar '{doc.Nombre}'?", "Sí", "No");
            if (confirmar)
            {
                await _databaseService.EliminarDocumentoAsync(doc);
                await CargarDocumentos();
            }
        }

        private void CancelarEdicion()
        {
            EstaEditando = false;
            _documentoEnEdicion = null;
            NombreDoc = string.Empty;
            NumeroDoc = string.Empty;
            FechaVencimiento = DateTime.Today;
            DiasAnticipacion = "1";
            _fotoActualBytes = null;
        }

        // --- LÓGICA DE FOTOS Y VISUALIZACIÓN ---
        private async Task TomarFoto()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo != null)
                    {
                        using var stream = await photo.OpenReadAsync();
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        _fotoActualBytes = memoryStream.ToArray();
                        await Application.Current!.MainPage!.DisplayAlert("Éxito", "Foto capturada", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task SeleccionarFoto()
        {
            try
            {
                FileResult? photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    using var stream = await photo.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    _fotoActualBytes = memoryStream.ToArray();
                    await Application.Current!.MainPage!.DisplayAlert("Éxito", "Imagen seleccionada", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task VerImagen(Documento documento)
        {
            if (documento?.DatosFoto == null || documento.DatosFoto.Length == 0)
            {
                await Application.Current!.MainPage!.DisplayAlert("Aviso", "Sin imagen adjunta.", "OK");
                return;
            }
            var imageSource = ImageSource.FromStream(() => new MemoryStream(documento.DatosFoto));
            var paginaImagen = new ContentPage
            {
                Title = documento.Nombre,
                BackgroundColor = Colors.Black,
                Content = new Grid { Children = { new Image { Source = imageSource, Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill } } }
            };
            if (Application.Current.MainPage is Shell shell) await shell.Navigation.PushAsync(paginaImagen);
            else await Application.Current.MainPage.Navigation.PushAsync(paginaImagen);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}