using AlDia.Services;
using AlDia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// Directivas necesarias para resolver errores de compilación
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace AlDia
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>() // Requiere Microsoft.Maui.Controls.Hosting
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Registro de servicios (Inyección de dependencias)
            // Requiere Microsoft.Extensions.DependencyInjection (incluido por defecto en el SDK de MAUI)
            builder.Services.AddSingleton<ServicioBaseDatos>();
            builder.Services.AddSingleton<IServicioNotificaciones, ServicioNotificaciones>();

            // Registro de ViewModels y Páginas
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}