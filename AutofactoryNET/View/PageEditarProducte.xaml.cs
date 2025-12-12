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

        }



        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            DataContext = _vmProducte;
            cmbTipus.SelectedIndex = 0;
            configuracioMode(mode);
            filtraNousSubcomponents();
            dtgBOM.ItemsSource = _vmProducte.Subcomponents;

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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (validaProducte())
            {
                Producte pValidat = new Producte(
                    _vmProducte.Codi,
                    _vmProducte.Nom,
                    _vmProducte.Descripcio,
                    _vmProducte.Stock,
                    _vmProducte.Foto
                    );

                OracleFactory.getUOW().DAOProducte.AfegirProducte(pValidat);
            }
        }

        private bool validaProducte()
        {
            throw new NotImplementedException();
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
                _vmProducte.Subcomponents.Add(_selectedNouComponente);
            }
        }

        private void btnEliminarSubcomp_Click(object sender, RoutedEventArgs e)
        {
            if (_vmProducte.Subcomponents.Contains(_selectedSubcomponente))
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
