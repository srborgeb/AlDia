using SQLite;
using AlDia.Models;
// Necesario para FileSystem
using Microsoft.Maui.Storage;

namespace AlDia.Services
{
    public class ServicioBaseDatos
    {
        private SQLiteAsyncConnection _conexion;

        private async Task Inicializar()
        {
            if (_conexion is not null)
                return;

            // FileSystem requiere Microsoft.Maui.Storage
            var rutaBaseDatos = Path.Combine(FileSystem.AppDataDirectory, "AlDiaDB.db3");

            _conexion = new SQLiteAsyncConnection(rutaBaseDatos);
            await _conexion.CreateTableAsync<Documento>();
        }

        public async Task<List<Documento>> ObtenerDocumentosAsync()
        {
            await Inicializar();
            return await _conexion.Table<Documento>().ToListAsync();
        }

        public async Task<int> GuardarDocumentoAsync(Documento documento)
        {
            await Inicializar();
            if (documento.Id != 0)
                return await _conexion.UpdateAsync(documento);
            else
                return await _conexion.InsertAsync(documento);
        }

        public async Task<int> EliminarDocumentoAsync(Documento documento)
        {
            await Inicializar();
            return await _conexion.DeleteAsync(documento);
        }
    }
}