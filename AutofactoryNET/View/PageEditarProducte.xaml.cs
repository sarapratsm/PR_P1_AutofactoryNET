using AppAutoFactory.ViewModel;
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
        int mode;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="mode">Si el mode es 0, és un nou producte. Si el mode és 1, és una edició</param>
        public PageEditarProducte(Producte p, int mode)
        {
            InitializeComponent();
            this.mode = mode;
            this._producte = p;
            this._vmProducte = new VMProducte(p);
            DataContext = _vmProducte;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if(NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void dtgComps_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void dtgComps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dtgSubcomponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnAfegirSubcomp_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
