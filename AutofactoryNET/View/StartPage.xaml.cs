using AutofactoryNET;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    /// Lógica de interacción para StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public MainWindow MainWindow { get; }

        public StartPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.MainWindow = mainWindow;
        }

        private void btnVeureProductes_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navegacio(1);

        }

        private void btnAfegirProducte_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnVeureComponents_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Navegacio(2);
        }

        private void btnAfegirComponent_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
