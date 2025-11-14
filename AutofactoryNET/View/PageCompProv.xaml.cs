using Model;
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

namespace AppAutoFactory.View
{
    /// <summary>
    /// Lógica de interacción para Comp_proveidor.xaml
    /// </summary>
    public partial class PageCompProv : Page
    {
        public PageCompProv()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void navigationService_LoadCompleted(object sender, NavigationEventArgs e)
        {
            object parametre = e.ExtraData;
            if (parametre != null || !(parametre is Componente)) throw new Exception("Parametre invàlid per a la pàgina Comp_proveidor.");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
