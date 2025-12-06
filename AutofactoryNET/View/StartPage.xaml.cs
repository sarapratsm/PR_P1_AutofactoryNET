using AutofactoryNET;
using DAO;
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
        Thread serverStatusThread;
        bool hiHaConnexio = false;
        private bool _keepRunning;

        public MainWindow MainWindow { get; }

        public StartPage(MainWindow mainWindow)
        {
            InitializeComponent();
            this.MainWindow = mainWindow;
            btnVeureComponents.IsEnabled = hiHaConnexio;
            btnVeureProductes.IsEnabled = hiHaConnexio;
        }

        private void btnVeureProductes_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow.Navegacio(1);
            intentarNavegar(1);

        }

        private void btnVeureComponents_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow.Navegacio(2);
            intentarNavegar(2);
        }

        private void intentarNavegar(int opcio)
        {
            if (hiHaConnexio)
                MainWindow.Navegacio(opcio);
            else
            {
                MessageBox.Show("No es pot connectar amb el servidor de base de dades. Comprova la connexió.", "Error de Connexió", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (serverStatusThread == null || !serverStatusThread.IsAlive)
            {
                serverStatusThread = new Thread(CheckServerStatus);
                serverStatusThread.IsBackground = true;
                _keepRunning = true;
                serverStatusThread.Start();
            }
        }
        //metode perque el thread comprovi si el servidor està actiu
        private void CheckServerStatus()
        {

            while (!hiHaConnexio && _keepRunning)
            {
                Dispatcher.Invoke(() =>
                {
                    lblServerStatus.Content = "Intentant connectar amb el servidor...";
                    lblServerStatus.Foreground = Brushes.Gray;
                });

                //El thread s'encalla aqui, així que suposo que millor fer dos dispatchers(?)
                string msg = "";
                bool serverIsUp = OracleFactory.getUOW().IsServerAlive(out msg);

                Dispatcher.Invoke(() =>
                {
                    lblServerStatus.Content = msg;
                    if (serverIsUp)
                    {
                        //TODO: mirar per què no agafa els colorsd.
                        lblServerStatus.Foreground = Brushes.Green;
                        hiHaConnexio = true;
                    }
                    else
                    {
                        lblServerStatus.Foreground = Brushes.Red;
                        hiHaConnexio = false;
                    }
                    btnVeureComponents.IsEnabled = hiHaConnexio;
                    btnVeureProductes.IsEnabled = hiHaConnexio;
                });
                Thread.Sleep(5000); // Espera 5 segons abans de tornar a comprovar
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _keepRunning = false; //aturem el fil quan sortim de la pàgina
        }
    }
}
