using Model;
using System.Collections.ObjectModel;

namespace DAO
{
    public interface IDAOProducte    
    {
        // --- Mètodes CRUD bàsics ---
        bool AfegirProducte(Producte producte);
        bool ModificarProducte(Producte producte);
        bool EliminarProducte(string codi);

        // --- Mètodes de Lectura ---
        Producte ObtenirProductePerCodi(string codi);
        ObservableCollection<Producte> ObtenirTots();
        ObservableCollection<Producte> GetProductesFiltrats(string codi, string nomParcial, int offset, int rows_per_page);

        //--- Mètodes Proveidor ---
        /// 
        /// <param name="p"></param>
        public bool AfegirProveidor(Proveidor p);

        /// 
        /// <param name="codi_prov"></param>
        public int EliminarProveidor(int codi_prov);

        //--- Mètodes de gestió de subproductes
        public ObservableCollection<Item> GetItems();

    }
}
