using SQLite;
using AlDia.Models;

namespace AlDia.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        async Task Init()
        {
            if (_database is not null)
                return;

            // Ruta de la base de datos en el dispositivo
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "AlDia.db3");

            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<Documento>();
        }

        public async Task<List<Documento>> GetDocumentosAsync()
        {
            await Init();
            return await _database.Table<Documento>().ToListAsync();
        }

        public async Task<int> SaveDocumentoAsync(Documento documento)
        {
            await Init();
            if (documento.Id != 0)
                return await _database.UpdateAsync(documento);
            else
                return await _database.InsertAsync(documento);
        }

        public async Task<int> DeleteDocumentoAsync(Documento documento)
        {
            await Init();
            return await _database.DeleteAsync(documento);
        }
    }
}