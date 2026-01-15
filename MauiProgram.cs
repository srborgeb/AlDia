using Microsoft.Extensions.Logging;
using AlDia.Services;
using AlDia.ViewModels;
using AlDia.Views;

namespace AlDia
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // REGISTRO DE SERVICIOS
            // Aquí corregimos el nombre: usamos DatabaseService en lugar de ServicioBaseDatos
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<ServicioNotificaciones>();

            // REGISTRO DE VIEWMODELS
            builder.Services.AddSingleton<MainViewModel>();

            // REGISTRO DE VISTAS (PÁGINAS)
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<PaginaDocumentos>();
            builder.Services.AddTransient<PaginaConfiguracion>();

            return builder.Build();
        }
    }
}