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
    }
}