using AppAutoFactory.View;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutofactoryNET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PageProds pageProds;
        public PageComps pageComps;
        public PageCompProv pageCompProv;

        public MainWindow()
        {
            InitializeComponent();
            pageProds = new PageProds();
            pageComps = new PageComps(this);
            pageCompProv = new PageCompProv();
        }

        private void mnuItems_Click(object sender, RoutedEventArgs e)
        {
        }

        private void mnuProductes_Click(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(pageProds);
        }

        private void mnuComponentes_Click(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(pageComps);
        }

        internal void NavegarACompProveidor(Model.Componente componente)
        {
            frmMain.Navigate(pageCompProv, componente);
        }

        
    }
}

