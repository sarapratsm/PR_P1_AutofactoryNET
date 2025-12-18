using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel; // <-- 1. Importar per a INotifyPropertyChanged
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices; // <-- 2. Importar per a [CallerMemberName]
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AppAutoFactory.ViewModel
{
    // 3. Implementar la interfície
    public class VMComponente : INotifyPropertyChanged
    {
        // 4. Implementar l'esdeveniment de la interfície
        public event PropertyChangedEventHandler? PropertyChanged;

        // 5. Mètode "Helper" per notificar a la UI
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // --- CAMPS PRIVATS (Backing Fields) ---
        private string codi;
        private string nom;
        private string descripcio;
        private int stock;
        private byte[]? foto;
        private char tipus;
        private string codiFabricant;
        private decimal preuMig;
        private Dictionary<Proveidor, decimal> proveidors_preu;
        ObservableCollection<VMProveidor> provsVM;
        private TipusUnitat unitat; // Canviat 'units' per 'unitat' per consistència
        private Componente componente;

        // --- PROPIETATS PÚBLIQUES (Per al Binding) ---
        // Ara, cada 'set' notifica a la UI que ha de refrescar-se

        public string Codi
        {
            get { return codi; }
            set { codi = value; OnPropertyChanged(); }
        }

        public string Nom
        {
            get { return nom; }
            set { nom = value; OnPropertyChanged(); }
        }

        public string Descripcio
        {
            get { return descripcio; }
            set { descripcio = value; OnPropertyChanged(); }
        }

        public int Stock
        {
            get { return stock; }
            set { stock = value; OnPropertyChanged(); }
        }

        public byte[]? Foto
        {
            get { return foto; }
            set { foto = value; this.TheImage = loadImage(Foto); OnPropertyChanged(); }
        }

        public BitmapImage TheImage
        {
            get;
            set;
        }

        public char Tipus
        {
            get { return tipus; }
            set { tipus = value; OnPropertyChanged(); }
        }

        public string CodiFabricant
        {
            get { return codiFabricant; }
            set { codiFabricant = value; OnPropertyChanged(); }
        }

        public decimal PreuMig
        {
            get { return preuMig; }
            set { preuMig = value; OnPropertyChanged(); }
        }

        public void RecalculaPreuMig()
        {
            if (this.preuMig == 0 && this.proveidors_preu != null && this.proveidors_preu.Count > 0)
            {
                // Calculamos la media (Average) de los valores del diccionario
                this.PreuMig = this.proveidors_preu.Values.Average();
            }
            else
            {
                this.PreuMig = 0;
            }
        }

        public Dictionary<Proveidor, decimal> Proveidors_preu
        {
            get { return proveidors_preu; }
            set { proveidors_preu = value; OnPropertyChanged(); }
        }

        public TipusUnitat Unitat // Canviat 'Units' per 'Unitat'
        {
            get { return unitat; }
            set { unitat = value; OnPropertyChanged(); }
        }

        // --- CONSTRUCTORS ---
        public VMComponente()
        {
            // Inicialitzem valors per defecte per evitar errors de 'null'
            codi = "";
            nom = "";
            descripcio = "";
            codiFabricant = "";
            proveidors_preu = new Dictionary<Proveidor, decimal>();
        }

        public VMComponente(Componente c)
        {
            // Assignem a les PROPIETATS (majúscula)
            this.Tipus = 'C';

            if (c != null)
            {
                this.componente = c;
                this.Codi = c.Codi;
                this.Nom = c.Nom;
                this.Descripcio = c.Descripcio;
                this.Stock = c.Stock;
                this.Foto = c.Foto;
                this.TheImage = loadImage(Foto);
                this.CodiFabricant = c.CodiFabricant;
                this.PreuMig = c.PreuMig;
                this.Proveidors_preu = c.Proveidors_preu;
                transformaDiccionariVM();
                this.Unitat = c.Unitat;
            }
            else
            {
                // Si 'c' és nul (p.ex. "Afegir Nou"), inicialitzem
                proveidors_preu = new Dictionary<Proveidor, decimal>();
            }
        }

        private BitmapImage loadImage(byte[] bytes)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                using (MemoryStream mem = new MemoryStream(bytes))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze(); // Importante para evitar errores de hilos
                return image;
            }
            catch
            {
                return null;
            }

        }

        public void save()
        {
            //si validacio correcta
            //si c!=null
            retransformaDiccionari();
        }

        public bool validarVM()
        {
            return false;
        }

        private bool transformaDiccionariVM()
        {
            this.provsVM = new ObservableCollection<VMProveidor>();
            if (this.proveidors_preu != null)
            {
                foreach (KeyValuePair<Proveidor, decimal> kvp in this.proveidors_preu)
                {
                    provsVM.Add(new VMProveidor(kvp.Key, kvp.Value));
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void retransformaDiccionari()
        {
            this.proveidors_preu.Clear();
            foreach (VMProveidor p in this.provsVM)
            {
                this.proveidors_preu.Add(p.Proveidor, p.Preu);
            }

        }
    }
}