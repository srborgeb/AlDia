namespace AlDia
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            // Cierra la aplicación
            Application.Current?.Quit();
        }
    }
}