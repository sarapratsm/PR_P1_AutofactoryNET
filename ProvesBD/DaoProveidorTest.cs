using DAO;
using Model;
using System.Collections.ObjectModel;

namespace ProvesBD
{
    [TestClass]
    public sealed class DaoProveidorTest
    {
        [TestMethod]
        public void Test_Lectura_ObtenirTotsProveidors_RetornaDadesDemoAmbJoins()
        {
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                // NO cal BeginTransaction() per a una lectura
                IDAOProveidor dao = uow.DAOProveidor;
                ObservableCollection<Proveidor> proveidors = null;

                try
                {
                    proveidors = dao.ObtenirTotsProveidors();
                }
                catch (System.Exception ex)
                {
                    Assert.Fail($"L'execució de ObtenirTotsProveidors ha fallat: {ex.Message}");
                }

                // Assert
                Assert.IsNotNull(proveidors, "La llista de proveïdors no hauria de ser nul·la.");

                // El teu script DadesDemo té 10 proveïdors (0 al 9)
                Assert.IsTrue(proveidors.Count >= 10, "S'esperaven almenys 10 proveïdors de les dades demo.");

                Proveidor prov1 = proveidors.FirstOrDefault(p => p.Codi == 1);
                Assert.IsNotNull(prov1, "El proveïdor amb Codi 1 (de les dades demo) no s'ha trobat.");

                Assert.IsNotNull(prov1.Municipi, "El municipi del proveïdor 1 no s'ha carregat (és nul).");
                Assert.AreEqual("Barcelona", prov1.Municipi.Nom, "El nom del municipi del proveïdor 1 no és 'Barcelona'.");

                Assert.IsNotNull(prov1.Municipi.Provincia, "La província del municipi no s'ha carregat (és nul).");
                Assert.AreEqual("Barcelona", prov1.Municipi.Provincia.Nom, "El nom de la província no és 'Barcelona'.");
            }
        }

        [TestMethod]
        public void Test_Escriptura_AfegirIObtenirProveidor_FuncionaCorrectament()
        {
            // Dades úniques per a aquest test
            int codiTest = 999;
            string cifTest = "TEST999CIF";

            // (El proveïdor es crea sense municipi per simplificar el test)
            Proveidor p = new Proveidor(codiTest, cifTest, "Proveidor de Test", "Carrer Fals 123", "Contacte Test", "555-1234", null);
            Proveidor proveidorCreat = null;

            // --- FASE 1: AFEGIR ---
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                try
                {
                    // 1. INICIEM LA TRANSACCIÓ MANUAL (perquè escriurem)
                    uow.BeginTransaction();

                    // 2. EXECUTEM L'ACCIÓ
                    IDAOProveidor dao = uow.DAOProveidor;
                    bool resultat = dao.AfegirProveidor(p);

                    // 3. VERIFIQUEM I FEM COMMIT
                    Assert.IsTrue(resultat, "El mètode AfegirProveidor hauria de retornar true.");
                    uow.Commit(); // Confirma la transacció
                }
                catch (Exception ex)
                {
                    uow.Rollback(); // Desfà si falla
                    Assert.Fail("AfegirProveidor ha fallat: " + ex.Message);
                }
            }

            // --- FASE 2: VERIFICAR QUE EXISTEIX ---
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                // (No cal transacció per llegir)
                try
                {
                    proveidorCreat = uow.DAOProveidor.ObtenirProveidor(codiTest);
                }
                catch (Exception ex)
                {
                    Assert.Fail("ObtenirProveidor ha fallat: " + ex.Message);
                }

                Assert.IsNotNull(proveidorCreat, "El proveïdor de test no s'ha trobat després de fer Commit.");
                Assert.AreEqual("Proveidor de Test", proveidorCreat.RaoSocial);
                Assert.AreEqual(cifTest, proveidorCreat.Cif);
                Assert.IsNull(proveidorCreat.Municipi, "El municipi hauria de ser nul.");
            }

            // --- FASE 3: NETEJA (ELIMINAR) ---
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                try
                {
                    uow.BeginTransaction();
                    IDAOProveidor dao = uow.DAOProveidor;
                    bool resultat = dao.EliminarProveidor(codiTest);
                    Assert.IsTrue(resultat, "El mètode EliminarProveidor hauria de retornar true.");
                    uow.Commit();
                }
                catch (Exception ex)
                {
                    uow.Rollback();
                    Assert.Fail("EliminarProveidor ha fallat: " + ex.Message);
                }
            }
        }
    }
}