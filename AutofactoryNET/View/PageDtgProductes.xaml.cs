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

        private int _paginaActual = 1;
        private int _elementosPorPagina = 10;
        private int _totalPaginas = 0;

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
            pageEditarProducte = null; 

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
                    MessageBox.Show("Selecciona un producte per modificar-lo!", "Atenció", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            if (pageEditarProducte != null)
            {
                this.NavigationService?.Navigate(pageEditarProducte);
            }
        }

        private void btnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            _paginaActual = 1;
            refreshDtgProductes();

        }

        private void refreshDtgProductes()
        {
            string filtreNom = txtFiltreNom.Text;
            string filtreCodi = txtFiltreCodi.Text;

            // Limpieza de nulos por si acaso
            if (filtreNom == null) filtreNom = "";
            if (filtreCodi == null) filtreCodi = "";

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    // 1. Contar TOTAL
                    int totalRegistros = uow.DAOProducte.ContarProductesFiltrats(filtreCodi, filtreNom);

                    // 2. Calcular páginas
                    if (totalRegistros == 0)
                    {
                        _totalPaginas = 1;
                        _paginaActual = 1;
                    }
                    else
                    {
                        _totalPaginas = (int)Math.Ceiling((double)totalRegistros / _elementosPorPagina);
                    }

                    // Ajustar página actual si nos hemos pasado
                    if (_paginaActual > _totalPaginas) _paginaActual = _totalPaginas;
                    if (_paginaActual < 1) _paginaActual = 1;

                    // 3. Calcular OFFSET
                    int offset = (_paginaActual - 1) * _elementosPorPagina;

                    // 4. Obtener datos paginados
                    // Asegúrate que tu DAOProducte.ObtenirProductesFiltrats usa OFFSET y FETCH NEXT internamente
                    llistaProductes = uow.DAOProducte.ObtenirProductesFiltrats(filtreCodi, filtreNom, offset, _elementosPorPagina);
                    dtgProductes.ItemsSource = llistaProductes;

                    // 5. Actualizar UI
                    ActualizarBotonesPaginacion();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en carregar els productes: \n" + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarBotonesPaginacion()
        {
            txtPagina.Text = $"{_paginaActual} / {_totalPaginas}";
            btnFirst.IsEnabled = _paginaActual > 1;
            btnPrevious.IsEnabled = _paginaActual > 1;
            btnNext.IsEnabled = _paginaActual < _totalPaginas;
            btnLast.IsEnabled = _paginaActual < _totalPaginas;
        }

        #region Paginacio


        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            _paginaActual = 1;
            refreshDtgProductes();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaActual > 1)
            {
                _paginaActual--;
                refreshDtgProductes();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaActual < _totalPaginas)
            {
                _paginaActual++;
                refreshDtgProductes();
            }
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            _paginaActual = _totalPaginas;
            refreshDtgProductes();
        }
        #endregion


    }
}
