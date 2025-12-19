using Model;
using System.Collections.ObjectModel;

namespace DAO
{
    public interface IDAOProducte    
    {
        // --- Mètodes CRUD bàsics ---
        bool InserirProducte(Producte producte);
        bool ModificarProducte(Producte producte);
        bool EliminarProducte(string codi);

        // --- Mètodes de Lectura ---
        Producte ObtenirProductePerCodi(string codi);
        ObservableCollection<Producte> ObtenirTots();
        public ObservableCollection<Producte> ObtenirProductesFiltrats(string codi_filtre, string nom_filtre, int offset = 0, int rows_per_page = -1);

        //--- Mètodes Proveidor ---
        /// 
        /// <param name="p"></param>
        public bool AfegirProveidor(Proveidor p);

        /// 
        /// <param name="codi_prov"></param>
        public int EliminarProveidor(int codi_prov);

        //--- Mètodes de gestió de subproductes
        public void CarregarSubcomponents(Producte p);
        public bool InserirSubcomponent(Producte p, Item i, int qt);
        public void EliminarTotsElsSubcomponents(string codiPare);

        public int ContarProductesFiltrats(string codi_filtre, string nom_filtre);


    }
}
