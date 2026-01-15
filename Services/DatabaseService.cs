using SQLite;
using AlDia.Models;

namespace AlDia.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        async Task Init()
        {
            if (_database is not null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "AlDia.db3");

            _database = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            await _database.CreateTableAsync<Documento>();
        }

        public async Task<List<Documento>> ObtenerDocumentosAsync()
        {
            await Init();
            return await _database!.Table<Documento>().ToListAsync();
        }

        public async Task<Documento> ObtenerDocumentoPorIdAsync(int id)
        {
            await Init();
            return await _database!.Table<Documento>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> GuardarDocumentoAsync(Documento documento)
        {
            await Init();
            if (documento.Id != 0)
                return await _database!.UpdateAsync(documento);
            else
                return await _database!.InsertAsync(documento);
        }

        public async Task<int> EliminarDocumentoAsync(Documento documento)
        {
            await Init();
            return await _database!.DeleteAsync(documento);
        }
    }
}