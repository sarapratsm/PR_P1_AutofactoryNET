using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using PDFQuest.DataAdapter;
//using PDFQuest.Template;

namespace AppAutoFactory.View
{
    /// <summary>
    /// Lógica de interacción para BOM.xaml
    /// </summary>
    public partial class PageVisorInforme : Page
    {
        public PageVisorInforme(string url)
        {
            InitializeComponent();
            InicializarNavegador(url);
        }

        private async void InicializarNavegador(string url)
        {
            // Inicialitzem el motor de Edge (WebView2)
            await BrowserView.EnsureCoreWebView2Async(null);
            // Naveguem a la URL del Jasper Report
            BrowserView.Source = new Uri(url);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if(NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

       
    }
}
