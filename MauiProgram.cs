using Microsoft.Extensions.Logging;
using AlDia.Services;
using AlDia.ViewModels;
using AlDia.Views; // Espacio de nombres para las vistas
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;

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

            // Registro de Servicios
            builder.Services.AddSingleton<ServicioBaseDatos>();
            builder.Services.AddSingleton<IServicioNotificaciones, ServicioNotificaciones>();

            // Registro de ViewModels
            builder.Services.AddSingleton<MainViewModel>();

            // Registro de Páginas (Necesario para que el Shell y la DI funcionen)
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<PaginaDocumentos>();
            builder.Services.AddSingleton<PaginaConfiguracion>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}