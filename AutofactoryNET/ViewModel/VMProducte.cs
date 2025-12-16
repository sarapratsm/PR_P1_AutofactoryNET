using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AppAutoFactory.ViewModel
{
    public class VMProducte : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string codi;
        private string nom;
        private string descripcio;
        private int stock;
        private byte[]? foto;
        private char tipus;
        private Dictionary<Item, Int32> subcomponents;
        private Producte producte;

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
            set { foto = value; OnPropertyChanged(); }
        }

        public char Tipus
        {
            get { return tipus; }
            set { tipus = value; OnPropertyChanged(); }
        }

        public Dictionary<Item, Int32> Subcomponents
        {
            get { return subcomponents; }
            set
            {
                subcomponents = value; OnPropertyChanged();
            }
        }

        public BitmapImage TheImage
        {
            get;
            set;
        }

        // --- CONSTRUCTORS ---
        public VMProducte()
        {
            // Constructor per a "Afegir Nou Producte"
            this.Tipus = 'P';
            this.codi = "";
            this.nom = "";
            this.descripcio = "";
            this.subcomponents = new Dictionary<Item, Int32>();
        }

        public VMProducte(Producte p)
        {
            // Constructor per a "Modificar Producte Existent"
            this.Tipus = 'P';

            if (p != null)
            {
                this.producte = p;
                this.Codi = p.Codi;
                this.Nom = p.Nom;
                this.Descripcio = p.Descripcio;
                this.Stock = p.Stock;
                this.Foto = p.Foto;
                this.TheImage = loadImage(Foto);
                // NOTA: Assegura't d'haver cridat a DAOProducte.CarregarSubcomponents(p) 
                // abans de passar-lo a aquest constructor.
                this.Subcomponents = p.Comps;
            }
            else
            {
                // Si per error p és null, inicialitzem buit per evitar errors
                this.subcomponents = new Dictionary<Item, Int32>();
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

    }
}