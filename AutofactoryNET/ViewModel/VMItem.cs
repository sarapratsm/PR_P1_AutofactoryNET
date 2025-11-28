/*using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.ComponentModel; // <-- 1. Importar per a INotifyPropertyChanged
//using System.Runtime.CompilerServices; // <-- 2. Importar per a [CallerMemberName]

namespace AppAutoFactory.ViewModel
{
    public class VMItem
    {
        private string codi;
        private string nom;
        private char tipus;
        private string descripcio;
        private Producte producte;
        private Componente componente;

        public VMItem(Producte p)
        {
            this.codi = p.Codi;
            this.nom = p.Nom;
            this.tipus= p.Tipus;
            this.descripcio = p.Descripcio;
            this.producte = p;

        }

        public VMItem(Componente c)
        {
            this.codi = c.Codi;
            this.nom = c.Nom;
            this.tipus = c.Tipus;
            this.descripcio = c.Descripcio;
            this.componente = c;
        }

        public string Codi { get => codi; }
        public string Nom { get => nom; }
        public char Tipus { get => tipus; }
        public string Descripcio { get => descripcio; }
       

    }
}
*/