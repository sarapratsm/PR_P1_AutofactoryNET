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
        private Componente _componentActual;
        ObservableCollection<KeyValuePair<Proveidor, decimal>> proveidorsDelComponent;

        // Aquesta col·lecció és per omplir el ComboBox
        ObservableCollection<Proveidor> totsElsProveidors;

        public PageCompProv()
        {
            InitializeComponent(); // Ara això no hauria de petar

            // Inicialitzem les col·leccions
            proveidorsDelComponent = new ObservableCollection<KeyValuePair<Proveidor, decimal>>();
            dtgCompProv.ItemsSource = proveidorsDelComponent;

            totsElsProveidors = new ObservableCollection<Proveidor>();
            cmbProveidors.ItemsSource = totsElsProveidors;
            cmbProveidors.DisplayMemberPath = "RaoSocial"; // Mostra el nom

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Obtenir el component passat com a paràmetre
            if (NavigationService != null && NavigationService.Content != null)
            {
                _componentActual = NavigationService.Content as Componente;
                if (_componentActual != null)
                {
                    lblComponent.Content = $"Proveïdors del component: {_componentActual.Nom}";
                    // Carregar els proveïdors del component
                    CarregarProveidorsDelComponent();
                    
                }
                else
                {
                    MessageBox.Show("No s'ha passat cap component vàlid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CarregarProveidorsDelComponent()
        {
            if (_componentActual == null) return;
            proveidorsDelComponent.Clear();
            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    // Cridem al mètode del DAO que ja hem implementat
                    var diccionari = uow.DAOComponente.GetProveidors(_componentActual.Codi);
                    foreach (var kvp in diccionari)
                    {
                        proveidorsDelComponent.Add(kvp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en carregar els proveïdors del component: \n" + ex.Message, "Error de BBDD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAfegirProveidor_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}