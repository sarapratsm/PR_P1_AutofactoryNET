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
using System.Windows.Navigation; // Necessari per a NavigationEventArgs

namespace AppAutoFactory.View
{
    public partial class PageCompProv : Page
    {
        private VMComponente _componentActual;
        
        ObservableCollection<Proveidor> llistaTotsProv; // Col·lecció és per omplir el ComboBox
        ObservableCollection<VMProveidor> _llistaVMProvs = new ObservableCollection<VMProveidor>(); //llista pel dtg.

        public PageCompProv(VMComponente c)
        {
            this._componentActual = c; //guardem el component
            InitializeComponent();
            CarregarProveidorsDelComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Combobox de proveidors
            llistaTotsProv = OracleFactory.getUOW().DAOProveidor.ObtenirTotsProveidors(); 
            cmbProveidors.ItemsSource = llistaTotsProv;
            cmbProveidors.DisplayMemberPath = "RaoSocial"; // Mostra el nom
            cmbProveidors.SelectedIndex = 0;

            //Datagrid:
            inicialitzarLlistaProvsVM();
            dtgCompProv.ItemsSource = _llistaVMProvs;

            lblComponent.Text = $"Proveïdors del component: {_componentActual.Nom}";
            // Carregar els proveïdors del component

        }

        private void inicialitzarLlistaProvsVM()
        {
            if (_componentActual.Proveidors_preu == null)
            {
                //Si la llista no s'ha carregat, la carreguem
                CarregarProveidorsDelComponent ();
            }
            transformaDiccionariVM(_componentActual.Proveidors_preu);
        }

        
        private void CarregarProveidorsDelComponent()
        {
            _componentActual.Proveidors_preu = new Dictionary<Proveidor, decimal>();
            _componentActual.Proveidors_preu = OracleFactory.getUOW().DAOComponente.ObtenirProveidorsPreuFiltrats(_componentActual.Codi, "", "");
        }

        private void transformaDiccionariVM(Dictionary<Proveidor, decimal> _llistaProvs)
        {
            _llistaVMProvs.Clear();
            foreach (KeyValuePair<Proveidor, decimal> kvp in _llistaProvs)
            {
                _llistaVMProvs.Add(new VMProveidor(kvp.Key, kvp.Value));
            }
        }

        private void filtrar()
        {
            string codi_filtre, cif_filtre;
            codi_filtre = txtCifFiltre.Text;
            cif_filtre = txtCifFiltre.Text;
            //Dictionary<Proveidor, decimal> ProvsFiltrats = OracleFactory.getUOW().DAOComponente.ObtenirProveidorsPreuFiltrats(_componentActual.Codi, codi_filtre, cif_filtre);
            //transformaDiccionariVM(ProvsFiltrats);

            //farem el filtre 

        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            //Transformar llista vmproveidors a la llista de proveidors, preu
            this._componentActual.retransformaDiccionari(); //Actualitzem el diccionari


        }

        private void btnAfegirProveidor_Click(object sender, RoutedEventArgs e)
        {
            Proveidor prov = (Proveidor)cmbProveidors.SelectedItem;
            Decimal preu_dec = 0;
            string preu = txtPreu.Text;
            bool v = Decimal.TryParse(preu, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.CurrentCulture, out preu_dec); //invocacion satanica
            if (prov == null)
            {
                MessageBox.Show("Selecciona un proveïdor vàlid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (preu_dec <= 0)
            {
                MessageBox.Show("Introdueix un preu vàlid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                if (_componentActual.Proveidors_preu.Keys.Any(p => p.Codi == prov.Codi)) //El contains no funciona pq busca la mateixa @de memoria
                {
                    MessageBox.Show("El proveïdor ja està assignat a aquest component.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    VMProveidor vmProv = new VMProveidor(prov, preu_dec);
                    _llistaVMProvs.Add(vmProv);
                    _componentActual.Proveidors_preu.Add(prov, preu_dec);
                }

            }



        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //retornar a la pagina anterior
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }

        }

        
    }
}