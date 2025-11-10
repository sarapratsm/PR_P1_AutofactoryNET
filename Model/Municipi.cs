using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Municipi
    {
        private int codi;
        private string nom;
        private Provincia provincia;
        private int provinciaCodi; // Propietat PK/FK (per a mu_pr_codi)

        public Municipi() { }
        public Municipi(int codi, string nom, Provincia provincia)
        {
            this.Codi = codi;
            this.Nom = nom;
            this.Provincia = provincia;
            if (provincia != null)
            {
                this.ProvinciaCodi = provincia.Codi;
            }
        }

        // Propietat de la Clau Primària (per a mu_codi)
        public int Codi { get => codi; private set => codi = value; }
        public string Nom { get => nom; private set => nom = value; }

        // Propietat de la Clau Primària (per a mu_pr_codi)
        public int ProvinciaCodi { get => provinciaCodi; private set => provinciaCodi = value; }

        // Propietat de Navegació
        public Provincia Provincia { get => provincia; private set => provincia = value; }
    }
}
