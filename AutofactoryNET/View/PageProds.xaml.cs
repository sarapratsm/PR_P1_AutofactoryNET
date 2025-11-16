using DAO;
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
using DAO;
using System.Collections.ObjectModel;
using Model;

namespace AppAutoFactory.View
{
    /// <summary>
    /// Lógica de interacción para PageProds.xaml
    /// </summary>
    public partial class PageProds : Page
    {
        ObservableCollection<Producte> productes = new ObservableCollection<Producte>();
        public PageProds()
        {
            InitializeComponent();
            dtgProductes.ItemsSource = productes;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            dtgProductes.ItemsSource = productes;

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    // Obtenim els productes de la BBDD
                    var productesDeLaBD = uow.DAOProducte.ObtenirTots();

                    foreach (var p in productesDeLaBD)
                    {
                        productes.Add(p);
                    }
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
