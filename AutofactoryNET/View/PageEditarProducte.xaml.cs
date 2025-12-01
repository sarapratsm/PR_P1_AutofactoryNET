using AppAutoFactory.ViewModel;
using DAO;
using Model;
using System;
using System.Collections.Generic;
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
        Producte _producte;
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
            this.mode = 0;
            this._producte = null;
            this._vmProducte = new VMProducte();
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
        }



        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _vmProducte;
            if (mode == 0)
            {
                txbTitol.Text = "Nou Producte";
            }
            else if (mode == 1)
            {
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

        private void btnFiltrar_Click(object sender, RoutedEventArgs e)
        {

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
