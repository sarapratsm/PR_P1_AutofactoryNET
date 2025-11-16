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
    /// Lógica de interacción para Comp_proveidor.xaml
    /// </summary>
    public partial class PageCompProv : Page
    {
        private Componente _componentActual;

        ObservableCollection<KeyValuePair<Proveidor, decimal>> proveidorsDelComponent; // Proveïdors i preus del component actual
        public PageCompProv()
        {
            InitializeComponent();
            proveidorsDelComponent = new ObservableCollection<KeyValuePair<Proveidor, decimal>>();
            dtgCompProv.ItemsSource = proveidorsDelComponent;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void navigationService_LoadCompleted(object sender, NavigationEventArgs e)
        {
            object parametre = e.ExtraData;
            if (parametre != null || !(parametre is Componente)) throw new Exception("Parametre invàlid per a la pàgina Comp_proveidor.");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (NavigationService.ExtraData is Componente componentRebut)
            {
                _componentActual = componentRebut;

                // (Ara que tenim el component, podem carregar les seves dades)
                CarregarProveidors();
            }
            else
            {
                MessageBox.Show("Error: No s'ha rebut un component vàlid.", "Error de Navegació", MessageBoxButton.OK, MessageBoxImage.Error);
                // Podríem tancar la pàgina si hi ha un error greu
                // this.NavigationService.GoBack();
            }
        }

        private void CarregarProveidors()
        {
            if (_componentActual == null) return;

            proveidorsDelComponent.Clear();

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    // Necessites implementar 'ObtenirProveidorsDeComponent' al teu DAO
                    var diccionari = uow.DAOComponente.GetProveidors(_componentActual);

                    foreach (var kvp in diccionari)
                    {
                        proveidorsDelComponent.Add(kvp);
                    }
                }
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("La funció 'ObtenirProveidorsDeComponent' encara no està implementada al DAO.", "Funció Pendent", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en carregar els proveïdors: \n" + ex.Message, "Error de BBDD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
