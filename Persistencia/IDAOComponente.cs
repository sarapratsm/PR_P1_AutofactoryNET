using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAO
{
    public interface IDAOComponente    
    {
        // --- Mètodes CRUD bàsics ---
        bool AfegirComponent(Componente component);
        bool ModificarComponent(Componente component);
        bool EliminarComponent(string codi);


        // --- Mètodes de Lectura ---
        Componente ObtenirComponentPerCodi(string codi);
        public ObservableCollection<Componente> GetComponents(string codi, string nom);
        public ObservableCollection<Componente> ObtenirTots();

        //--- Mètodes Proveidor ---
        public Dictionary<Proveidor, decimal> GetProveidors(string codi_component);
        public decimal CalculaPreuMig(Componente component);

        //public List<Component> ObtenirPerFabricant(string fabricant);

        //public List<Component> ObtenirPerRangPreu(double min, double max);

        //public bool Existeix(string codi);

    }
}
