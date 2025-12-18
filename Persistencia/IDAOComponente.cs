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
        public ObservableCollection<Componente> ObtenirComponentsFiltrats(string codi, string nom, int offset = 0, int rows_per_page = -1);


        //public ObservableCollection<Componente> ObtenirTotsESBORRAR(); //Utilitzarem el filtrat sense filtres

        //--- Mètodes Proveidor ---
        public Dictionary<Proveidor, decimal> ObtenirProveidorsPreuFiltrats(string codi_comp, string cif_filtre, string raoSocial_filtre, int offset = 0, int rows_per_page = -1); 
        public Dictionary<string, decimal> ObternirProvCodPreu(Componente component); //Aquest mètode és més senzill

        public decimal CalculaPreuMig(Componente component); //Aquest mètode no cal perquè el preu mig es calcula amb un trigger a la BD

        //public List<Component> ObtenirPerFabricant(string fabricant);

        //public List<Component> ObtenirPerRangPreu(double min, double max);

        //public bool Existeix(string codi);

        public void EliminarProveidorsDeComponent(string codiComponent);
        public void AfegirProveidorAComponent(string codiComponent, int codiProveidor, decimal preu);



    }
}
