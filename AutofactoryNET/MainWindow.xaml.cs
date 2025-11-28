using AppAutoFactory.View;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Model;
using System.Collections.ObjectModel;
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
        

        public PageDtgProductes pageProds;
        public PageComps pageComps;
        public PageCompProv pageCompProv;
        public StartPage pageInici;



        public MainWindow()
        {
            InitializeComponent();
            pageProds = new PageDtgProductes();
            pageComps = new PageComps(this);
            pageInici = new StartPage(this);

        }

        private void mnuInici_Click(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(pageInici);
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

        /// <summary>
        /// Mètode per navegar entre les pàgines principals
        /// </summary>
        /// <param name="opcio"> 0:Start, 1:Productes, 2:Components </param>
        internal void Navegacio(int opcio)
        {
            switch (opcio)
            {
                case 0:
                    frmMain.Navigate(pageInici);
                    break;
                case 1:
                    frmMain.Navigate(pageProds);
                    break;
                case 2:
                    frmMain.Navigate(pageComps);
                    break;
                default:
                    break;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (frmMain != null)
            {
                frmMain.Navigate(pageInici);
            }
        }

        private void frmMain_Navigated(object sender, NavigationEventArgs e)
        {
            // Aquí pots afegir codi que s'executa després de cada navegació, si cal
            if (frmMain != null)
            {
                if (frmMain.Content is PageDtgProductes)
                {
                    txbTitle.Text = "Productes";
                }
                else if (frmMain.Content is PageComps)
                {
                    txbTitle.Text = "Components";
                }
                else if (frmMain.Content is StartPage)
                {
                    txbTitle.Text = "AutoFactory";
                }
                else if (frmMain.Content is PageCompProv)
                {
                    txbTitle.Text = "Proveïdors";
                }
            }
        }
    }
}

