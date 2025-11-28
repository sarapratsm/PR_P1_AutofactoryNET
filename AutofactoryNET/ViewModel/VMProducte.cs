using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- 1. Importar per a INotifyPropertyChanged
using System.Runtime.CompilerServices; // <-- 2. Importar per a [CallerMemberName]
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAutoFactory.ViewModel
{
    // 3. Implementar la interfície
    internal class VMProducte : INotifyPropertyChanged
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
        // El Diccionari de Components (Comps) el gestionarem per separat,
        //[cite_start]// ja que és per al DataGrid de la pàgina 5 
        private Producte producte;

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
            set { foto = value; OnPropertyChanged(); }
        }

        public char Tipus
        {
            get { return tipus; }
            set { tipus = value; OnPropertyChanged(); }
        }

        // --- CONSTRUCTORS ---
        public VMProducte()
        {
            // Constructor per a "Afegir Nou Producte"
            this.Tipus = 'P';
            this.codi = "";
            this.nom = "";
            this.descripcio = "";
        }

        public VMProducte(Producte p)
        {
            // Constructor per a "Modificar Producte Existent"
            this.Tipus = 'P';

            if (p != null)
            {
                this.producte= p;
                // Assignem a les PROPIETATS (majúscula)
                this.Codi = p.Codi;
                this.Nom = p.Nom;
                this.Descripcio = p.Descripcio;
                this.Stock = p.Stock;
                this.Foto = p.Foto;
            }
        }
    }
}