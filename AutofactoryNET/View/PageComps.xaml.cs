using AppAutoFactory.ViewModel;
using AutofactoryNET;
using DAO;
using Microsoft.Win32;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        //ObservableCollection<Componente> llistaComponents = new ObservableCollection<Componente>();

        private Componente _componenteActual;
        private VMComponente _vmComponente;
        int mode = 0; //0:Afegir, 1:Modificar


        public MainWindow MainWindow { get; }



        public PageComps(MainWindow mainWindow)
        {
            InitializeComponent();
            this.MainWindow = mainWindow;
            DataContext = _vmComponente;

        }




        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //llistaComponents = OracleFactory.getUOW().DAOComponente.ObtenirComponentsFiltrats("", "");
            //dtgComps.ItemsSource = llistaComponents;
            RefrescarDataGrid();
            //dtgComps.ItemsSource = llistaComponents;

            List<TipusUnitat> tipusUnitats = Enum.GetValues(typeof(TipusUnitat)).Cast<TipusUnitat>().ToList();
            cmbUnitats.ItemsSource = tipusUnitats;

            try
            {
                RefrescarDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en carregar els productes des de la BBDD: \n" + ex.Message,
                                "Error de Connexió", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void RefrescarDataGrid()
        {
            // Buidem la llista actual
            //llistaComponents.Clear();
            string codi = txtFiltreCodi.Text;
            string nom = txtFiltreNom.Text;

            if (string.IsNullOrWhiteSpace(codi))
            {
                codi = "";
            }
            if (string.IsNullOrWhiteSpace(nom))
            {
                nom = "";
            }

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    dtgComps.ItemsSource = uow.DAOComponente.ObtenirComponentsFiltrats(codi, nom).ToList();                
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
            //Crear nou component
            _vmComponente = new VMComponente();
            DataContext = _vmComponente;
            txbNom.IsEnabled = true;
            mode = 0; //Afegir
            
        }



        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            //Eliminar component seleccionat
            Componente compSeleccionat = dtgComps.SelectedItem as Componente;
            if (compSeleccionat == null)
            {
                MessageBox.Show("Si us plau, selecciona un component per eliminar.", "Cap Selecció", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MessageBoxResult result = MessageBox.Show($"Estàs segur que vols eliminar el component amb codi '{compSeleccionat.Codi}'?",
                                                      "Confirmar Eliminació", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                {
                try
                {
                    using (UnitOfWork uow = OracleFactory.getUOW())
                    {
                        uow.BeginTransaction();
                        uow.DAOComponente.EliminarComponent(compSeleccionat.Codi);
                        uow.Commit();
                    }
                    RefrescarDataGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error en eliminar el component de la BBDD: \n" + ex.Message,
                                    "Error en l'eliminació", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //Validar dades
            //No poden ser null el codi, nom ni tipus
            if (string.IsNullOrEmpty(_vmComponente.Codi))
            {
                MessageBox.Show("El codi del component no pot estar buit.", "Dades Invàlides", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(_vmComponente.Nom))
            {
                MessageBox.Show("El nom del component no pot estar buit.", "Dades Invàlides", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if(cmbUnitats.SelectedItem == null)
            {
                MessageBox.Show("Si us plau, selecciona una unitat vàlida.", "Dades Invàlides", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Componente nouComp = new Componente(_vmComponente.Codi,
                                                    _vmComponente.Nom,
                                                    _vmComponente.Descripcio,
                                                    _vmComponente.Stock,
                                                    _vmComponente.Foto,
                                                    _vmComponente.Tipus,
                                                    _vmComponente.CodiFabricant,
                                                    _vmComponente.Unitat);
            //Guardar dades a la BBDD
            if (mode == 0) //Afegir
            {
                //Comprovar que no existeix un component amb el mateix codi
                Componente compExist = OracleFactory.getUOW().DAOComponente.ObtenirComponentPerCodi(_vmComponente.Codi);
                if (compExist != null)
                {
                    MessageBox.Show("Ja existeix un component amb aquest codi. Si us plau, utilitza un codi diferent.", "Codi Duplicat", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    
                    try
                    {
                        //Afegir a la BBDD
                        using (UnitOfWork uow = OracleFactory.getUOW())
                        {
                            uow.BeginTransaction();
                            uow.DAOComponente.AfegirComponent(nouComp);
                            uow.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error en afegir el component a la BBDD: \n" + ex.Message,
                                        "Error en la inserció", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;

                    }
                }

            }else if (mode == 1) //Modificar
            {
                //Actualitzar a la BBDD
                try
                {
                    using (UnitOfWork uow = OracleFactory.getUOW())
                    {
                        uow.BeginTransaction();
                        uow.DAOComponente.ModificarComponent(nouComp);
                        uow.Commit();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error en actualitzar el component a la BBDD: \n" + ex.Message,
                                    "Error en l'actualització", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


            }
            //Refrescar DataGrid
            RefrescarDataGrid();
        }

        private void netejaFiltres()
        {
            txtFiltreCodi.Text = "";
            txtFiltreNom.Text = "";
        }

        

        private void dtgComps_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnEditarProvs_Click(sender, e);
        }

        private void dtgComps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _componenteActual = (Componente)dtgComps.SelectedItem;
            _vmComponente = new VMComponente(_componenteActual);
            DataContext = _vmComponente;
            txbNom.IsEnabled = false;
            mode=1; //Modificar


        }

        private void btnEditarProvs_Click(object sender, RoutedEventArgs e)
        {
            if(_componenteActual == null)
            {
                MessageBox.Show("Si us plau, selecciona un component per gestionar els seus proveïdors.", "Cap Selecció", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //Obrir PageCompProv passant el Component seleccionat
            PageCompProv pageCompProv = new PageCompProv(_vmComponente);
            this.NavigationService.Navigate(pageCompProv);

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }

        }

        private void btnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            RefrescarDataGrid();
        }

        

        private void btnRemoveFoto_Click(object sender, RoutedEventArgs e)
        {
            _vmComponente.Foto = null;
        }

        private void btnPujarFoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Filtramos solo imágenes
            openFileDialog.Filter = "Imatges|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 1. Llegim el fitxer del disc i el convertim a bytes
                    byte[] imatgeBytes = File.ReadAllBytes(openFileDialog.FileName);

                    // 2. Assignem els bytes al ViewModel
                    // En ser PropertyChanged, la imatge es refrescarà sola
                    _vmComponente.Foto = imatgeBytes;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error en carregar la imatge: " + ex.Message);
                }
            }
        }

        private void btnGestioProvs_Click(object sender, RoutedEventArgs e)
        {
            btnEditarProvs_Click(sender, e);
        }
    }
}
