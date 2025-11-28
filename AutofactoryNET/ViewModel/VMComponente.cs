using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- 1. Importar per a INotifyPropertyChanged
using System.Linq;
using System.Runtime.CompilerServices; // <-- 2. Importar per a [CallerMemberName]
using System.Text;
using System.Threading.Tasks;

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
            set { foto = value; OnPropertyChanged(); }
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
                this.CodiFabricant = c.CodiFabricant;
                this.PreuMig = c.PreuMig;
                this.Proveidors_preu = c.Proveidors_preu;
                this.Unitat = c.Unitat;
            }
            else
            {
                // Si 'c' és nul (p.ex. "Afegir Nou"), inicialitzem
                proveidors_preu = new Dictionary<Proveidor, decimal>();
            }
        }

        public void save()
        {
            //si validacio correcta

            //si c!=null
        }

        public bool validarVM()
        {
            return false;
        }
    }
}