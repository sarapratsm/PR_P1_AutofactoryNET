using DAO;
using Model;
using System.Collections.ObjectModel;

namespace ProvesBD
{
    [TestClass]
    public sealed class DaoComponenteTest
    {
        [TestMethod]
        public void Test_Lectura_ObtenirTotsComponents_RetornaDadesDemo()
        {
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                var components = uow.DAOComponente.ObtenirTots();
                Assert.IsNotNull(components);
                Assert.IsTrue(components.Count >= 10, "S'esperaven almenys 10 components.");
            }
        }

        [TestMethod]
        public void Test_Escriptura_AfegirComponent_Funciona()
        {
            // --- CANVI: Codi més curt ("C-TEST-1" té 8 caràcters) ---
            string codiTest = "C-TEST-1";
            Componente c = new Componente(codiTest, "Test Afegir Comp", "Desc Comp", 50, null, 'C', "FAB-TEST", TipusUnitat.Unitats);

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    uow.BeginTransaction();
                    bool res = uow.DAOComponente.AfegirComponent(c);
                    Assert.IsTrue(res, "AfegirComponent hauria de retornar true");
                    uow.Commit();
                }
            }
            finally
            {
                // Neteja (S'executa sempre)
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    uow.BeginTransaction();
                    uow.DAOComponente.EliminarComponent(codiTest);
                    uow.Commit();
                }
            }
        }

        [TestMethod]
        public void Test_Escriptura_ModificarComponent_Funciona()
        {
            // --- CANVI: Codi més curt ("C-TEST-2" té 8 caràcters) ---
            string codiTest = "C-TEST-2";
            Componente cOrig = new Componente(codiTest, "Test Modificar Comp", "Desc Orig", 10, null, 'C', "FAB-ORIG", TipusUnitat.Kg);

            try
            {
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    uow.BeginTransaction();
                    uow.DAOComponente.AfegirComponent(cOrig);
                    uow.Commit();
                }

                Componente cMod = new Componente(codiTest, "Nom Modificat", "Desc Mod", 99, null, 'C', "FAB-MOD", TipusUnitat.Unitats);
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    uow.BeginTransaction();
                    bool res = uow.DAOComponente.ModificarComponent(cMod);
                    Assert.IsTrue(res, "ModificarComponent hauria de retornar true");
                    uow.Commit();
                }

                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    var cVerificar = uow.DAOComponente.ObtenirComponentPerCodi(codiTest);
                    Assert.IsNotNull(cVerificar);
                    Assert.AreEqual("Nom Modificat", cVerificar.Nom);
                    Assert.AreEqual("FAB-MOD", cVerificar.CodiFabricant);
                }
            }
            finally
            {
                // Neteja (S'executa sempre)
                using (UnitOfWork uow = OracleFactory.getUOW())
                {
                    uow.BeginTransaction();
                    uow.DAOComponente.EliminarComponent(codiTest);
                    uow.Commit();
                }
            }
        }
    }
}