using System.Linq;

namespace Model
{
    public class Producte : Item
    {
        public Producte() : base(){}

        private Dictionary<Item, decimal> _comps;

        public Producte(string codi, string nom, string desc, int stock, byte[] foto)
            : base(codi, nom, desc, stock, foto, 'P')
        {
            _comps = new Dictionary<Item, decimal>();
        }

        // Aquest Dictionary no es pot mapejar a la BD.
        // L'haurem d'ignorar al 'OnModelCreating' de MyDBContext.
        public Dictionary<Item, decimal> Comps { get => _comps; private set => _comps = value; }
        public void AfegirComponent(Item c, decimal qt)
        {
            // 1. Comprueba si la CLAVE existe
            if (!_comps.ContainsKey(c))
            {
                // 2. Añade la CLAVE (c) y un VALOR (p.ej., 1)
                _comps.Add(c, qt);
            }
            // Si ya existe, este código no hace nada,
            // que es lo que tu 'if' original implicaba.
        }

        // CORREGIDO:
        public void EliminarComponent(Item c)
        {
            // 1. Comprueba si la CLAVE existe
            if (_comps.ContainsKey(c))
            {
                // 2. Elimina usando la CLAVE
                _comps.Remove(c);
            }
        }
    }
}
