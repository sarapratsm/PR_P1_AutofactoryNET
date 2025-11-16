using AutofactoryNET;
using DAO;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Lógica de interacción para Components.xaml
    /// </summary>
    public partial class PageComps : Page
    {
        ObservableCollection<Componente> componentes = new ObservableCollection<Componente>();
        public MainWindow MainWindow { get; } // Per poder navegar

        public PageComps()
        {
            InitializeComponent();
        }

        public PageComps(MainWindow mainWindow)
        {
            InitializeComponent();
            this.MainWindow = mainWindow;
        }


        private void dtgComps_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Navegar cap a Page Comp_proveidor passant el component per parametre.
            btnModificar_Click(sender, null);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            dtgComps.ItemsSource = componentes;
            componentes= OracleFactory.getUOW().DAOComponente.ObtenirTots();

        }

        private void RefrescarDataGrid()
        {
            // Buidem la llista actual
            componentes.Clear();

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    // Obtenim els components de la BBDD
                    var componentsDeLaBD = uow.DAOComponente.ObtenirTots();

                    // Omplim la col·lecció
                    foreach (var c in componentsDeLaBD)
                    {
                        componentes.Add(c);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en carregar els components des de la BBDD: \n" + ex.Message,
                                "Error de Connexió", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            Componente compSeleccionat = dtgComps.SelectedItem as Componente;
            if (compSeleccionat == null)
            {
                MessageBox.Show("Si us plau, selecciona un component per modificar.", "Cap Selecció", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.MainWindow != null)
            {
                this.MainWindow.NavegarACompProveidor(compSeleccionat);
            }
        }
    }
}
