using AppAutoFactory.ViewModel;
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
        ObservableCollection<Componente> llistaComponents = new ObservableCollection<Componente>();

        private Componente _componenteActual;
        private VMComponente _vmComponente;
        

        public MainWindow MainWindow { get; } 

        

        public PageComps(MainWindow mainWindow)
        {
            InitializeComponent();
            this.MainWindow = mainWindow;
            DataContext = _vmComponente;

        }




        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            llistaComponents= OracleFactory.getUOW().DAOComponente.ObtenirComponentsFiltrats("", "");
            dtgComps.ItemsSource = llistaComponents;
    

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    // Obtenim els productes de la BBDD
                    var componentesDeLaBD = uow.DAOComponente.ObtenirComponentsFiltrats("", "");

                    foreach (var c in componentesDeLaBD)
                    {
                        llistaComponents.Add(c);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en carregar els productes des de la BBDD: \n" + ex.Message,
                                "Error de Connexió", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void RefrescarDataGrid(string codi, string nom)
        {
            // Buidem la llista actual
            llistaComponents.Clear();

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    // Obtenim els components de la BBDD
                    var componentsDeLaBD = uow.DAOComponente.ObtenirComponentsFiltrats(codi, nom);

                    // Omplim la col·lecció
                    foreach (var c in componentsDeLaBD)
                    {
                        llistaComponents.Add(c);
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

        private void btnAfegir_Click(object sender, RoutedEventArgs e)
        {

        }

       

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAfegirProv_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dtgComps_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            

        }

        private void dtgComps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _componenteActual =(Componente) dtgComps.SelectedItem;
            _vmComponente = new VMComponente(_componenteActual);
            DataContext = _vmComponente;

        }

        private void btnEditarProvs_Click(object sender, RoutedEventArgs e)
        {
            //Obrir PageCompProv passant el Component seleccionat
            PageCompProv pageCompProv = new PageCompProv(_componenteActual);
            this.NavigationService.Navigate(pageCompProv);

        }

        private void btnGestioProvs_Click(object sender, RoutedEventArgs e)
        {

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
