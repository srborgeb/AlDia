namespace AlDia
{
    public partial class App : Application
    {
        // El constructor recibe MainPage gracias a la inyección de dependencias
        public App(MainPage mainPage)
        {
            InitializeComponent();

            // Usamos NavigationPage para tener una barra de título bonita
            MainPage = new NavigationPage(mainPage);
        }
    }
}
