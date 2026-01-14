using AlDia.ViewModels;
using Microsoft.Maui.Controls;

namespace AlDia
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var vm = BindingContext as MainViewModel;
            vm?.CargarDocumentosCommand.Execute(null);
        }
    }
}