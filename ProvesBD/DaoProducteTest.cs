using DAO;
using Model;
using System.Collections.ObjectModel;
using Oracle.EntityFrameworkCore;

namespace ProvesBD
{
    [TestClass]
    public sealed class DaoProducteTest
    {
        [TestMethod]
        public void Test_Lectura_ObtenirTotsProductes_RetornaDadesDemo()
        {
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                var productes = uow.DAOProducte.ObtenirTots();
                Assert.IsNotNull(productes);
                Assert.IsTrue(productes.Count >= 4, "S'esperaven almenys 4 productes.");
            }
        }

        [TestMethod]
        public void Test_Escriptura_CicleCompletProducte_AfegirObtenirEliminar()
        {
            // --- CANVI: Codi més curt ("P-TEST-1" té 8 caràcters) ---
            string codiTest = "P-TEST-1";
            Producte p = new Producte(codiTest, "Producte Test", "Desc Test", 10, null);

            try
            {
                // --- FASE 1: AFEGIR ---
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    uow.BeginTransaction();
                    bool resultat = uow.DAOProducte.AfegirProducte(p);
                    Assert.IsTrue(resultat, "El mètode AfegirProducte hauria de retornar true.");
                    uow.Commit();
                }

                // --- FASE 2: VERIFICAR QUE EXISTEIX ---
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    var producteCreat = uow.DAOProducte.ObtenirProductePerCodi(codiTest);
                    Assert.IsNotNull(producteCreat, "El producte de test no s'ha trobat.");
                    Assert.AreEqual("Producte Test", producteCreat.Nom);
                }
            }
            finally
            {
                // --- FASE 3: NETEJA (S'EXECUTA SEMPRE) ---
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    uow.BeginTransaction();
                    uow.DAOProducte.EliminarProducte(codiTest);
                    uow.Commit();
                }
            }
        }

        [TestMethod]
        public void Test_Lectura_GetProductesFiltrats_RetornaDadesCorrectes()
        {
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                IDAOProducte dao = uow.DAOProducte;
                ObservableCollection<Producte> resultats;

                // Cas 1: Sense filtres (hauria de retornar-ho tot)
                // El teu script de dades demo té 4 productes
                resultats = dao.GetProductes("", "");
                Assert.IsTrue(resultats.Count >= 4, "S'esperaven almenys 4 productes sense filtre.");

                // Cas 2: Filtre per Codi parcial (PDF)
                // Hauria de trobar PDF_01 i PDF_03
                resultats = dao.GetProductes("PDF", "");
                Assert.AreEqual(2, resultats.Count, "Hauria de trobar 2 productes amb codi 'PDF'.");

                // Cas 3: Filtre per Nom parcial (fre)
                // Hauria de trobar els 4
                resultats = dao.GetProductes("", "fre");
                Assert.AreEqual(4, resultats.Count, "Hauria de trobar 4 productes amb nom 'fre'.");

                // Cas 4: Filtre per Codi i Nom exacte
                resultats = dao.GetProductes("PDF_01", "Pinça de fre");
                Assert.AreEqual(1, resultats.Count, "Hauria de trobar 1 producte amb codi 'PDF_01' i nom 'Pinça de fre'.");
                Assert.AreEqual("PDF_01", resultats[0].Codi);

                // Cas 5: Filtre sense resultats
                resultats = dao.GetProductes("XXXXX", "YYYYY");
                Assert.AreEqual(0, resultats.Count, "No hauria de trobar cap producte.");
            }
        }
    }
}