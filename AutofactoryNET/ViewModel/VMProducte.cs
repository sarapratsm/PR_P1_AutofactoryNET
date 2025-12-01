using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- 1. Importar per a INotifyPropertyChanged
using System.Runtime.CompilerServices; // <-- 2. Importar per a [CallerMemberName]
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

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
        private ObservableCollection<Item> subcomponents;
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

        public ObservableCollection<Item> Subcomponents
        {
            get { return subcomponents; }
            set
            {
                subcomponents = value; OnPropertyChanged();
            }
        }

        // --- CONSTRUCTORS ---
        public VMProducte()
        {
            // Constructor per a "Afegir Nou Producte"
            this.Tipus = 'P';
            this.codi = "";
            this.nom = "";
            this.descripcio = "";
            this.subcomponents = new ObservableCollection<Item>();

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
                this.subcomponents= new ObservableCollection<Item>(); //Caldrà omplir la llista desde la app.

            }
        }
    }
}