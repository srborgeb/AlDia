using AlDia.ViewModels;

namespace AlDia.Views
{
    public partial class PaginaDocumentos : ContentPage
    {
        public PaginaDocumentos(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}