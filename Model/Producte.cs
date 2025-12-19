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
            if (!_comps.ContainsKey(c))
            {
                _comps.Add(c, qt);
            }
        }

        public void EliminarComponent(Item c)
        {
            if (_comps.ContainsKey(c))
            {
                _comps.Remove(c);
            }
        }
    }
}
