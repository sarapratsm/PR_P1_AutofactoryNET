using DAO;
using Model;
using System.Collections.ObjectModel;

namespace ProvesBD
{
    [TestClass]
    public sealed class DaoProveidorTest
    {
        /// <summary>
        /// Comprova que 'ObtenirTotsProveidors' retorna les dades demo
        /// i que ha carregat correctament (amb JOINs) els Municipis i Províncies.
        /// </summary>
        [TestMethod]
        public void Test_Lectura_ObtenirTotsProveidors_RetornaDadesDemoAmbJoins()
        {
            // Arrange
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                IDAOProveidor dao = uow.DAOProveidor;
                List<Proveidor> proveidors = null;

                // Act
                try
                {
                    proveidors = dao.ObtenirTotsProveidors();
                }
                catch (System.Exception ex)
                {
                    Assert.Fail($"L'execució de ObtenirTotsProveidors ha fallat: {ex.Message}");
                }

                // Assert (Nivell 1: Recompte)
                Assert.IsNotNull(proveidors, "La llista de proveïdors no hauria de ser nul·la.");

                // Comprovem que hi ha 8 proveïdors, segons el DadesDemo.sql
                Assert.IsTrue(proveidors.Count >= 8, "S'esperaven almenys 8 proveïdors de les dades demo.");

                // Assert (Nivell 2: Dades Específiques)
                // Busquem el proveïdor 1, que ha d'estar a Barcelona
                Proveidor prov1 = proveidors.FirstOrDefault(p => p.Codi == 1);
                Assert.IsNotNull(prov1, "El proveïdor amb Codi 1 (de les dades demo) no s'ha trobat.");

                // Assert (Nivell 3: Comprovació del JOIN a Municipi)
                Assert.IsNotNull(prov1.Municipi, "El municipi del proveïdor 1 no s'ha carregat (és nul).");
                Assert.AreEqual("Barcelona", prov1.Municipi.Nom, "El nom del municipi del proveïdor 1 no és 'Barcelona'.");

                // Assert (Nivell 4: Comprovació del JOIN niat a Provincia)
                Assert.IsNotNull(prov1.Municipi.Provincia, "La província del municipi no s'ha carregat (és nul).");
                Assert.AreEqual("Barcelona", prov1.Municipi.Provincia.Nom, "El nom de la província no és 'Barcelona'.");
            }
        }
    }
}
