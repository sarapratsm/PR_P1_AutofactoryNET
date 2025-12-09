using AppAutoFactory.ViewModel;
using DAO;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
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
    /// Lógica de interacción para PageDtgProductes.xaml
    /// </summary>
    public partial class PageDtgProductes : Page
    {
        ObservableCollection<Producte> llistaProductes = new ObservableCollection<Producte>();
        private Producte? _producteSeleccionat;
        //private VMProducte _producteVM;

        PageEditarProducte pageEditarProducte;

        public PageDtgProductes()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            dtgProductes.ItemsSource = llistaProductes;

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    // Obtenim els productes de la BBDD
                    var productesDeLaBD = uow.DAOProducte.ObtenirTots();

                    foreach (var p in productesDeLaBD)
                    {
                        llistaProductes.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en carregar els productes des de la BBDD: \n" + ex.Message,
                                "Error de Connexió", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void btnAfegirProducte_Click(object sender, RoutedEventArgs e)
        {
            NavegaAEditorProducte(0);
        }

        private void dtgProductes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NavegaAEditorProducte(1);
        }
        private void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            NavegaAEditorProducte(1);

        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            //mostrar un missatge que demani confirmació de la eliminació.
            //MessageBox.Show("S'eliminarà el producte. Aquest canvi és irreversible");
        }
        
        private void dtgProductes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _producteSeleccionat = dtgProductes.SelectedItem as Producte;

        }

        private void NavegaAEditorProducte(int mode)
        {
            if (mode == 0)
            {
                pageEditarProducte = new PageEditarProducte();
            }
            else if (mode == 1)
            {
                if (_producteSeleccionat != null)
                {
                    pageEditarProducte = new PageEditarProducte(_producteSeleccionat);
                }
            }
            this.NavigationService?.Navigate(pageEditarProducte);
        }

        private void btnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            string filtreNom = txtFiltreNom.Text;
            string filtreCodi = txtFiltreCodi.Text;


            ObservableCollection<Producte> productesFiltrats = OracleFactory.getUOW().DAOProducte.ObtenirProductesFiltrats(filtreCodi, filtreNom, -1, 10);
            dtgProductes.ItemsSource = productesFiltrats;

        }
    }
}
