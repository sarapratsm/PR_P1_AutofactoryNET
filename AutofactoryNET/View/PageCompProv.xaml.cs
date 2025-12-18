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

        ObservableCollection<Proveidor> _llistaTotsProv; // Col·lecció és per omplir el ComboBox
        ObservableCollection<VMProveidor> _llistaVMProvs = new ObservableCollection<VMProveidor>(); //llista pel dtg.
        private VMProveidor? _selectedVMProveidor;

        public PageCompProv(VMComponente c)
        {
            this._componentActual = c; //guardem el component
            InitializeComponent();
            CarregarProveidorsDelComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Combobox de proveidors
            _llistaTotsProv = OracleFactory.getUOW().DAOProveidor.ObtenirTotsProveidors();
            cmbProveidors.ItemsSource = _llistaTotsProv;
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
            //_llistaVMProvs = new ObservableCollection<VMProveidor>();
            if (_componentActual.Proveidors_preu == null)
            {
                //Si la llista no s'ha carregat, la carreguem
                CarregarProveidorsDelComponent();
            }
            _llistaVMProvs = TransformaDiccionariVM(_componentActual.Proveidors_preu);
        }


        private void CarregarProveidorsDelComponent()
        {
            _componentActual.Proveidors_preu = new Dictionary<Proveidor, decimal>();
            _componentActual.Proveidors_preu = OracleFactory.getUOW().DAOComponente.ObtenirProveidorsPreuFiltrats(_componentActual.Codi, "", "");
        }

        private ObservableCollection<VMProveidor> TransformaDiccionariVM(Dictionary<Proveidor, decimal> _llistaProvs)
        {
            ObservableCollection<VMProveidor> llista = new ObservableCollection<VMProveidor>();
            foreach (KeyValuePair<Proveidor, decimal> kvp in _llistaProvs)
            {
                llista.Add(new VMProveidor(kvp.Key, kvp.Value));
            }
            return llista;

        }



        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            //Transformar llista vmproveidors a la llista de proveidors, preu
            //this._componentActual.retransformaDiccionari(); //Actualitzem el diccionari
            foreach (VMProveidor vmProv in _llistaVMProvs)
            {
                if (!_componentActual.Proveidors_preu.ContainsKey(vmProv.Proveidor))
                {
                    _componentActual.Proveidors_preu.Add(vmProv.Proveidor, vmProv.Preu);
                }
            }

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    uow.BeginTransaction();
                    uow.DAOComponente.EliminarProveidorsDeComponent(_componentActual.Codi);
                    foreach (KeyValuePair<Proveidor, decimal> kvp in _componentActual.Proveidors_preu)
                    {
                        uow.DAOComponente.AfegirProveidorAComponent(_componentActual.Codi, kvp.Key.Codi, kvp.Value);
                    }
                    _componentActual.RecalculaPreuMig();
                    uow.Commit();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: "+ex.Message, "Error en guardar les dades", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }


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

        #region botons de filtrat
        private void filtraCombo_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Proveidor> provsFiltratsCmb = new ObservableCollection<Proveidor>();
            string filtreCif = txtCifFiltre.Text.ToLower();
            string filtreRaoSocial = txtRaoSocial.Text.ToLower();

            foreach (Proveidor p in _llistaTotsProv)
            {
                if (p.RaoSocial.ToLower().Contains(filtreRaoSocial) && p.Cif.ToLower().Contains(filtreCif))
                {
                    provsFiltratsCmb.Add(p);
                }
            }
            cmbProveidors.ItemsSource = provsFiltratsCmb;
        }

        private void filtraDtg_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<Proveidor, decimal> DiccionariProvsFiltratsDtg = new Dictionary<Proveidor, decimal>();

            string filtreRaoSocial = txtRaoSocial.Text.ToLower();
            string filtreCif = txtCifFiltre.Text.ToLower();

            DiccionariProvsFiltratsDtg = OracleFactory.getUOW().DAOComponente.ObtenirProveidorsPreuFiltrats(_componentActual.Codi, filtreCif, filtreRaoSocial);
            ObservableCollection<VMProveidor> llista_filtrada = TransformaDiccionariVM(DiccionariProvsFiltratsDtg);
            dtgCompProv.ItemsSource = llista_filtrada;



        }

        private void netejaFiltres_Click(object sender, RoutedEventArgs e)
        {
            cmbProveidors.ItemsSource = _llistaTotsProv;
            dtgCompProv.ItemsSource = _llistaVMProvs;
        }
        #endregion

        private void btnEliminarProveidor_Click(object sender, RoutedEventArgs e)
        {
            //Eliminar un proveidor 
            if (_selectedVMProveidor != null)
            {
                _componentActual.Proveidors_preu.Remove(_selectedVMProveidor.Proveidor);
            }
        }

        private void dtgCompProv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dtgCompProv.SelectedItem != null)
            {
                _selectedVMProveidor = dtgCompProv.SelectedItem as VMProveidor;
            }
        }
    }
}