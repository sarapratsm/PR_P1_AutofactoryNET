using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public abstract class Item
    {
        //campos
        private string codi;
        private string nom ;
        private string descripcio;
        private int stock  ;
        private byte[] foto;
        private char tipus;

        protected Item() { }

        protected Item(string codi, string nom, string desc, int stock, byte[] foto, char tipus)
        {
            this.Codi = codi;
            this.Nom = nom;
            this.Desc = desc;
            this.Stock = stock;
            this.Foto = foto;
            this.Tipus = tipus;
        }

        public string Codi { get => codi; private set => codi = value; }
        public string Nom { get => nom; private set => nom = value; }
        public string Desc { get => descripcio; private set => descripcio = value; }
        public int Stock { get => stock; set => stock = value; } // Deixem 'set' públic per a Stock
        public byte[] Foto { get => foto; private set => foto = value; }
        public char Tipus { get => tipus; private set => tipus = value; }


        
    }
}
