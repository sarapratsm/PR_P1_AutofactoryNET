using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Componente : Item
    {
        //camps
        private string codiFabricant = string.Empty;
        private decimal preuMig;
        private Dictionary<Proveidor, decimal> proveidors_preu = new Dictionary<Proveidor, decimal>();
        private TipusUnitat units;

        public Componente() : base()
        {
        }

        public Componente(string codi, string nom, string desc, int stock, byte[] foto, char tipus,
                     string codiFabricant, TipusUnitat unitat)
        : base(codi, nom, desc, stock, foto, tipus)
        {
            this.CodiFabricant = codiFabricant;
            this.Unitat = unitat;
            proveidors_preu = new Dictionary<Proveidor, decimal>();
            // Ya inicializados arriba
            PreuMig = 0; // Es calcularà amb el Trigger de la BD
        }

        public string CodiFabricant { get => codiFabricant; private set => codiFabricant = value; }
        public decimal PreuMig { get => preuMig; private set => preuMig = value; } // EF el podrà omplir

        public Dictionary<Proveidor, decimal> Proveidors_preu { get => proveidors_preu;  set => proveidors_preu = value; }

        public TipusUnitat Unitat { get => units; private set => units = value; }
    }
}
