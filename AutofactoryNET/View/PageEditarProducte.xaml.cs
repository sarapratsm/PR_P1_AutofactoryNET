using AppAutoFactory.ViewModel;
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
    /// Lógica de interacción para EditorProductes.xaml
    /// </summary>
    public partial class PageEditarProducte : Page
    {
        Producte? _producte;
        VMProducte _vmProducte;
        int mode; //0 és un nou producte. 1 és una edició.
        private Item _selectedSubcomponente;
        private Item _selectedNouComponente;




        /// <summary>
        /// Constructor per a la creació i eedició d'un producte nou.
        /// </summary>
        public PageEditarProducte()
        {
            InitializeComponent();
            this._producte = null;
            this._vmProducte = new VMProducte();
            DataContext = _vmProducte;
            this.mode = 0;

        }



        /// <summary>
        /// Constructor per a la edició d'un producte ja existent.
        /// </summary>
        /// <param name="p">És el producte a editar</param>
        public PageEditarProducte(Producte p)
        {
            InitializeComponent();
            this.mode = 1;
            this._producte = p;
            this._vmProducte = new VMProducte(p);
            DataContext = _vmProducte;
            _vmProducte.Subcomponents = OracleFactory.getUOW().DAOProducte.obtenirSubcomponents(_producte);

        }



        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            DataContext = _vmProducte;
            cmbTipus.SelectedIndex = 0;
            configuracioMode(mode);
            filtraNousSubcomponents();
            
            dtgBOM.ItemsSource = _vmProducte.Subcomponents.Keys;

        }

        private void filtraNousSubcomponents()
        {
            string filtreCodi = txbFiltreCodi.Text;
            string filtreNom = txbFiltreNom.Text;
            //No pot apareixer el component que s'està editant.
            if (cmbTipus.SelectedIndex == 0)
            {
                //Components
                List<Componente> lComps = OracleFactory.getUOW().DAOComponente.ObtenirComponentsFiltrats(filtreCodi, filtreNom).ToList();
                ObservableCollection<Item> oComps = new ObservableCollection<Item>(lComps);
                dtgItems.ItemsSource = oComps;
            }
            else if (cmbTipus.SelectedIndex == 1)
            {
                //Productes
                ObservableCollection<Producte> lProds = OracleFactory.getUOW().DAOProducte.ObtenirProductesFiltrats(filtreCodi, filtreNom);
                if (_producte != null)
                {

                    lProds.Remove(lProds.FirstOrDefault(p => p.Codi == _producte.Codi));

                }
                ObservableCollection<Item> oProds = new ObservableCollection<Item>(lProds);
                dtgItems.ItemsSource = oProds;
            }
        }

        private void configuracioMode(int nouMode)
        {
            this.mode = nouMode; //0 és un nou producte. 1 és una edició.
            //Admet canvis en els controls segons el mode.
            //Per exemple, el codi no es pot canviar en mode edició.
            txbCodi.IsEnabled = nouMode == 0;

            if (mode == 0)
            {
                //NOU PRODUCTE
                txbTitol.Text = "Nou Producte";
                btnGuardar.Content = "Crear Producte";
            }
            else if (mode == 1)
            {
                //EDICIÓ PRODUCTE
                btnGuardar.Content = "Desar Canvis";
                txbTitol.Text = "Edició del producte " + _producte.Codi;
            }



        }



        private bool validaItem()
        {
            StringBuilder sb = new StringBuilder();
            //nou producte
            string codi = txbCodi.Text;
            if (codi.Length > 10 || codi.Length < 1)
            {
                sb.Append("El codi no és vàlid (Mínim 1 caracter i màxim 10). \n");
            }
            string nom = txbNom.Text;
            if (nom.Length > 100 || nom.Length < 1)
            {
                sb.Append("El nom no és vàlid (Mínim 1 caracter i màxim 100). \n");
            }
            string desc = txbDesc.Text;
            if (desc.Length > 255 || desc.Length < 1)
            {
                sb.Append("La descripció no és vàlida (Mínim 1 caracter i màxim 255). \n");
            }
            int stock = 0;
            try
            {
                stock = Int32.Parse(txbStock.Text);

            }
            catch (Exception ex)
            {
                sb.Append("Stock ha de ser un número enter! \n");
            }

            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString(), "Producte no vàlid", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }





        private void dtgItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgItems.SelectedItem is Componente)
                _selectedNouComponente = dtgItems.SelectedItem as Componente;
            else if (dtgItems.SelectedItem is Producte)
                _selectedNouComponente = dtgItems.SelectedItem as Producte;

            btnAfegirSubcomp.IsEnabled = _selectedNouComponente != null;

        }

        private void dtgSubcomponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgBOM.SelectedItem is Componente)
                _selectedSubcomponente = dtgBOM.SelectedItem as Componente;
            else if (dtgBOM.SelectedItem is Producte)
                _selectedSubcomponente = dtgBOM.SelectedItem as Producte;

        }

        private void btnAfegirSubcomp_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNouComponente != null)
            {
                _vmProducte.Subcomponents.Add(_selectedNouComponente, Int32.Parse(txtQuantitatSubcomp.Text));
            }
        }

        private void btnEliminarSubcomp_Click(object sender, RoutedEventArgs e)
        {
            if (_vmProducte.Subcomponents.Keys.Contains(_selectedSubcomponente))
            {
                _vmProducte.Subcomponents.Remove(_selectedSubcomponente);
            }
        }

        private void cmbTipus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filtraNousSubcomponents();
        }
        private void btnFiltrar_Click(object sender, RoutedEventArgs e)
        {
            filtraNousSubcomponents();
        }




        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Producte pValidat = null;
            if (validaItem())
            {
                try
                {
                    pValidat = new Producte(
                    _vmProducte.Codi,
                    _vmProducte.Nom,
                    _vmProducte.Descripcio,
                    _vmProducte.Stock,
                    _vmProducte.Foto
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error en crear el producte", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


            }
            if (mode == 0)
            {
                try
                {
                    using (UnitOfWork uow = OracleFactory.getUOW())
                    {
                        uow.BeginTransaction();
                        uow.DAOProducte.AfegirProducte(pValidat);
                        foreach (KeyValuePair i in _vmProducte.Subcomponents)
                            uow.DAOProducte.AfegirSubcomponent(_producte, i.key, i.value);
                        uow.Commit();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("No s'ha pogut guardar el producte: " + ex.Message, "Error en guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
            else if (mode == 1)
            {
                //producte editat
                try
                {
                    using (UnitOfWork uow = OracleFactory.getUOW())
                    {
                        uow.BeginTransaction();
                        uow.DAOProducte.ModificarProducte(pValidat);
                        uow.Commit();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No s'ha pogut guardar el producte: " + ex.Message, "Error en guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
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
