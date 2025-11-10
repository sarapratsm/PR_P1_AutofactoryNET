using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Proveidor
    {
        // --- Camps Privats ---
        private int codi;
        private string cif;
        private string raoSocial;
        private string adrFacturacio;
        private string persona_contacte;
        private string telfContacte;
        private Municipi municipi;

        /// <summary>
        /// Constructor buit necessari per a Entity Framework Core.
        /// </summary>
        public Proveidor() { }

        // Constructor complet actualitzat
        public Proveidor(int codi, string cif, string raoSocial, string adrFacturacio, string persona_contacte, string telfContacte, Municipi municipi)
        {
            this.Codi = codi;
            this.Cif = cif;
            this.RaoSocial = raoSocial;
            this.AdrFacturacio = adrFacturacio;
            this.Persona_contacte = persona_contacte;
            this.TelfContacte = telfContacte;
            this.Municipi = municipi;
        }

        // --- Propietats Públiques (amb 'private set' per a EF) ---
        public int Codi { get => codi; private set => codi = value; }
        public string Cif { get => cif; private set => cif = value; }
        public string RaoSocial { get => raoSocial; private set => raoSocial = value; }
        public string AdrFacturacio { get => adrFacturacio; private set => adrFacturacio = value; }
        public string Persona_contacte { get => persona_contacte; private set => persona_contacte = value; }
        public string TelfContacte { get => telfContacte; private set => telfContacte = value; }

        // Propietat de Navegació (ara gestiona la FK de forma oculta)
        public Municipi Municipi { get => municipi; private set => municipi = value; }
    }
}