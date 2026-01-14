namespace AlDia
{
    // Este archivo es vital para corregir el error CS0311 en MauiProgram.cs
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}