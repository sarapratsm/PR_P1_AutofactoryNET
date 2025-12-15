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

            refreshDtgProductes();

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
            if (_producteSeleccionat != null)
            {
                MessageBoxResult result = MessageBox.Show("Estàs segur que vols eliminar el producte seleccionat?", "Confirmació d'eliminació", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (UnitOfWork uow = OracleFactory.getUOW())
                        {
                            uow.BeginTransaction();
                            uow.DAOProducte.EliminarProducte(_producteSeleccionat.Codi);
                            uow.Commit();
                        }
                        llistaProductes.Remove(_producteSeleccionat);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error en eliminar el producte de la BBDD: \n" + ex.Message,
                                        "Error de Connexió", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Si us plau, selecciona un producte per eliminar-lo.", "Cap producte seleccionat", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
                else
                {
                    MessageBox.Show("Selecciona un producte per modificar-lo!");
                }
            }
            this.NavigationService?.Navigate(pageEditarProducte);
        }

        private void btnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            refreshDtgProductes();

        }

        private void refreshDtgProductes()
        {
            string filtreNom = txtFiltreNom.Text;
            string filtreCodi = txtFiltreCodi.Text;


            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    llistaProductes = uow.DAOProducte.ObtenirProductesFiltrats(filtreCodi, filtreNom, -1, 10);
                    dtgProductes.ItemsSource = llistaProductes;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en carregar els productes des de la BBDD: \n" + ex.Message,
                                "Error de Connexió", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
