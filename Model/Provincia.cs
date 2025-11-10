using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Provincia
    {
        private int codi;
        private string nom;

        /// <summary>
        /// Constructor buit necessari per a Entity Framework Core.
        /// </summary>
        public Provincia() { }

        public Provincia(int codi, string nom)
        {
            this.Codi = codi;
            this.Nom = nom;
        }

        public int Codi { get => codi; private set => codi = value; }
        public string Nom { get => nom; private set => nom = value; }
    }
}