using System;
using System.Windows;
using System.Windows.Controls;

namespace AppAutoFactory.View
{
    public partial class PageVisorInforme : Page
    {
        public PageVisorInforme(string url)
        {
            InitializeComponent();
            InicializarNavegador(url);
        }

        private async void InicializarNavegador(string url)
        {
            try
            {
                // Intentem inicialitzar el motor del navegador
                await BrowserView.EnsureCoreWebView2Async(null);

                // Si va bé, naveguem
                BrowserView.Source = new Uri(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crític al carregar el visor PDF (WebView2).\n\n" +
                                $"Detall: {ex.Message}\n\n" +
                                "Assegura't que tens instal·lat el 'Microsoft Edge WebView2 Runtime'.",
                                "Error de Visor", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}