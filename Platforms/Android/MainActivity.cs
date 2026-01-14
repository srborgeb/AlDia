using Android.App;
using Android.Content.PM;
using Android.OS;
// Necesario para resolver MauiAppCompatActivity
using Microsoft.Maui;
using Plugin.LocalNotification;
using System.Reflection.Metadata;

namespace AlDia
{
    [Activity(Theme = "@style/Maui.MainTheme.NoActionBar", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var canalId = "recordatorios_aldia";
                var nombre = "Recordatorios Al Día";
                var importancia = NotificationImportance.High;

                NotificationChannel canal = new NotificationChannel(canalId, nombre, importancia)
                {
                    Description = "Notificaciones de documentos pendientes"
                };

                // GetSystemService y NotificationService son métodos/propiedades de la base Android Activity
                var gestorNotificaciones = (NotificationManager)GetSystemService(NotificationService);
                gestorNotificaciones.CreateNotificationChannel(canal);
            }
        }
    }
}