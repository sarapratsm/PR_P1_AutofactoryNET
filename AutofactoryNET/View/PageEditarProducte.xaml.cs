using AppAutoFactory.ViewModel;
using DAO;
using Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;


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
            OracleFactory.getUOW().DAOProducte.CarregarSubcomponents(_producte);
            this._vmProducte = new VMProducte(p);
            DataContext = _vmProducte;


        }



        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            DataContext = _vmProducte;
            cmbTipus.SelectedIndex = 0;
            configuracioMode(mode);
            filtraNousSubcomponents();

            dtgBOM.DataContext = _vmProducte.Subcomponents;
            RefrescarDtgBOM();

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



        private bool validaItem()
        {
            StringBuilder sb = new StringBuilder();
            //nou producte
            string codi = txbCodi.Text;
            if (codi.Length > 10 || codi.Length < 1)
            {
                sb.Append("El codi no és vàlid (Mínim 1 caracter i màxim 10). \n");
            }
            string nom = txbNom.Text;
            if (nom.Length > 100 || nom.Length < 1)
            {
                sb.Append("El nom no és vàlid (Mínim 1 caracter i màxim 100). \n");
            }
            string desc = txbDesc.Text;
            if (desc.Length > 255 || desc.Length < 0)
            {
                sb.Append("La descripció no és vàlida (Màxim 255 caracters). \n");
            }
            int stock = 0;
            if (!int.TryParse(txbStock.Text, out stock) || stock < 0)
            {
                sb.Append("L'Stock ha de ser un número enter vàlid i no negatiu.\n");
            }

            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString(), "Producte no vàlid", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }





        private void dtgItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedNouComponente = dtgItems.SelectedItem as Item;

            // Activem o desactivem el botó segons el resultat
            btnAfegirSubcomp.IsEnabled = _selectedNouComponente != null;
        }

        private void dtgBOM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtgBOM.SelectedItem != null)
            {
                var filaSeleccionada = (KeyValuePair<Item, decimal>)dtgBOM.SelectedItem;
                _selectedSubcomponente = filaSeleccionada.Key;
            }
            else
            {
                _selectedSubcomponente = null;
            }
            btnEliminarSubcomp.IsEnabled = _selectedSubcomponente != null;


        }



        /// <summary>
        /// Mètode per refrescar la taula visualment
        /// </summary>
        private void RefrescarDtgBOM()
        {
            dtgBOM.ItemsSource = null;
            dtgBOM.ItemsSource = _vmProducte.Subcomponents.ToList();
        }

        private void btnAfegirSubcomp_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedNouComponente != null)
            {
                string textQuantitat = txtQuantitatSubcomp.Text.Replace(",", ".");

                decimal quantitat = 0;

                // Usamos decimal.TryParse con InvariantCulture
                bool esValid = decimal.TryParse(textQuantitat,
                                                System.Globalization.NumberStyles.Any,
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                out quantitat);


                if (!esValid || quantitat <= 0)
                {
                    MessageBox.Show("La quantitat ha de ser un número vàlid i major que 0 (ex: 1.5).", "Error de format", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_vmProducte.Subcomponents.Any(i => i.Key.Codi == _selectedNouComponente.Codi))
                {
                    MessageBox.Show("Error: Aquest item ja està especificat en el BOM del producte!", "No es pot afegir l'item", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _vmProducte.Subcomponents.Add(_selectedNouComponente, quantitat);
                RefrescarDtgBOM();

                txtQuantitatSubcomp.Text = "1"; // Reset

            }
        }

        private void btnEliminarSubcomp_Click(object sender, RoutedEventArgs e)
        {
            if (_vmProducte.Subcomponents.Keys.Contains(_selectedSubcomponente))
            {
                _vmProducte.Subcomponents.Remove(_selectedSubcomponente);
            }
            RefrescarDtgBOM();
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
            Producte pValidat = null;
            if (validaItem())
            {
                try
                {
                    pValidat = new Producte(
                    _vmProducte.Codi,
                    _vmProducte.Nom,
                    _vmProducte.Descripcio,
                    _vmProducte.Stock,
                    _vmProducte.Foto
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error en crear el producte", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


            }
            else
            {
                return;
            }
            //El producte és correcte.
            if (mode == 0)
            {
                try
                {
                    using (UnitOfWork uow = OracleFactory.getUOW())
                    {
                        uow.BeginTransaction();

                        uow.DAOProducte.InserirProducte(pValidat);

                        foreach (var kvp in _vmProducte.Subcomponents)
                        {
                            uow.DAOProducte.InserirSubcomponent(pValidat, kvp.Key, kvp.Value);
                        }

                        uow.Commit();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No s'ha pogut guardar el producte: " + ex.Message,
                                    "Error en guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (mode == 1)
            {
                //producte editat
                try
                {
                    using (UnitOfWork uow = OracleFactory.getUOW())
                    {
                        uow.BeginTransaction();
                        uow.DAOProducte.ModificarProducte(pValidat);
                        uow.DAOProducte.EliminarTotsElsSubcomponents(pValidat.Codi);
                        foreach (var kvp in _vmProducte.Subcomponents)
                        {
                            uow.DAOProducte.InserirSubcomponent(pValidat, kvp.Key, kvp.Value);
                        }
                        uow.Commit();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No s'ha pogut guardar el producte: " + ex.Message, "Error en guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void btnRemoveFoto_Click(object sender, RoutedEventArgs e)
        {
            _vmProducte.Foto = null;
        }

        private void btnPujarFoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Filtramos solo imágenes
            openFileDialog.Filter = "Imatges|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 1. Llegim el fitxer del disc i el convertim a bytes
                    byte[] imatgeBytes = File.ReadAllBytes(openFileDialog.FileName);

                    // 2. Assignem els bytes al ViewModel
                    // En ser PropertyChanged, la imatge es refrescarà sola
                    _vmProducte.Foto = imatgeBytes;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error en carregar la imatge: " + ex.Message);
                }
            }
        }

        public async Task<string> ObtenirInformeProducte(string codiProducte)
        {
            var config = DAO.MyDBContext.GetJasperConfig();

            if (string.IsNullOrEmpty(config.BaseUrl))
            {
                MessageBox.Show("No s'ha pogut llegir la configuració de Jasper al appsettings.json.", "Error de Configuració", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string BASE_URL = config.BaseUrl + "/rest_v2/reports";
            string REPORT_PATH = "/AutoFactory/Reports/BOM_Producte/BOMProducte";

            try
            {
                string url = $"{BASE_URL}{REPORT_PATH}.pdf?codi_producte={codiProducte}";

                using (HttpClient client = new HttpClient())
                {
                    var authValue = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.User}:{config.Pass}")));

                    client.DefaultRequestHeaders.Authorization = authValue;

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();

                        string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Fitxa_{codiProducte}_{DateTime.Now.Ticks}.pdf");
                        File.WriteAllBytes(tempFile, pdfBytes);

                        return tempFile;
                    }
                    else
                    {
                        MessageBox.Show($"Error Jasper ({response.StatusCode}): No s'ha pogut generar l'informe.", "Error Servidor", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de connexió amb Jasper: " + ex.Message);
                return null;
            }
            finally
            {
                btnImprimirBOM.IsEnabled = true;
                lblLoading.Content = "";  
            }
        }



        // Importante: Añadimos 'async' aquí para poder esperar a la descarga
        private async void ImprimirBOM_Click(object sender, RoutedEventArgs e)
        {
            if (_producte == null) return;

            btnImprimirBOM.IsEnabled = false;
            lblLoading.Content = "Carregant informe...";

            //Llamamos al método y ESPERAMOS (await) a que devuelva la ruta
            string rutaPdfLocal = await ObtenirInformeProducte(_producte.Codi);

            if (!string.IsNullOrEmpty(rutaPdfLocal))
            {
                PageVisorInforme paginaInforme = new PageVisorInforme(rutaPdfLocal);
                this.NavigationService.Navigate(paginaInforme);
            }
        }
    }
}
