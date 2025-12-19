using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel; 
using System.Runtime.CompilerServices;

namespace AppAutoFactory.ViewModel
{
    internal class VMProveidor : INotifyPropertyChanged
    {
        private int codi;
        private string cif;
        private string raoSocial;
        private string adrFacturacio;
        private string persona_contacte;
        private string telfContacte;
        private Municipi municipi;
        private decimal preu; //camp addicional per al preu del proveidor
        private Proveidor proveidor;

        public VMProveidor(Proveidor prov, decimal preu)
        {
            this.Codi = prov.Codi;
            this.Cif = prov.Cif;
            this.RaoSocial = prov.RaoSocial;
            this.AdrFacturacio = prov.AdrFacturacio;
            this.Persona_contacte = prov.Persona_contacte;
            this.TelfContacte = prov.TelfContacte;
            this.Municipi = prov.Municipi;
            this.Preu = preu;
            this.proveidor = prov;
        }

        #region propietats
        public int Codi
        {
            get { return codi; }
            set { codi = value; }
        }
        public string Cif
        {
            get { return cif; }
            set { cif = value; }
        }
        public string RaoSocial
        {
            get { return raoSocial; }
            set { raoSocial = value; }
        }
        public string AdrFacturacio
        {
            get { return adrFacturacio; }
            set { adrFacturacio = value; }
        }
        public string Persona_contacte
        {
            get { return persona_contacte; }
            set { persona_contacte = value; }
        }
        public string TelfContacte
        {
            get { return telfContacte; }
            set { telfContacte = value; }
        }
        public Municipi Municipi
        {
            get { return municipi; }
            set { municipi = value; }
        }
        public decimal Preu
        {
            get { return preu; }
            set { preu = value; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Proveidor Proveidor
        {
            get { return proveidor; }
            set { proveidor = value; }
        }
        #endregion
    }
}
