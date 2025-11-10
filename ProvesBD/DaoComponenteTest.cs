using DAO;
using Model;
using System.Collections.ObjectModel;

namespace ProvesBD
{
    [TestClass]
    public sealed class DaoComponenteTest
    {
        /// <summary>
        /// Comprova que 'ObtenirTotsComponents' retorna les dades
        /// de demo inserides amb l'script SQL.
        /// </summary>
        [TestMethod]
        public void Test_Lectura_ObtenirTotsComponents_RetornaDadesDemo()
        {
            // Arrange
            // 'using' s'encarrega de fer .Dispose() automàticament
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                // Obtenim el DAO des de la UnitOfWork
                IDAOComponente dao = uow.DAOComponente;

                List<Componente> components = null;

                // Act
                try
                {
                    components = dao.ObtenirTots();
                }
                catch (System.Exception ex)
                {
                    // Si la connexió o la consulta SQL falla, el test fallarà
                    Assert.Fail($"L'execució de ObtenirTotsComponents ha fallat: {ex.Message}");
                }

                // Assert
                Assert.IsNotNull(components, "La llista de components no hauria de ser nul·la.");

                // Segons el teu script, hi ha 10 components
                Assert.IsTrue(components.Count >= 10, "S'esperaven almenys 10 components de les dades demo.");

                // Comprovem si existeix un component específic
                bool existeixRF = components.Any(c => c.Codi == "RF1123"); //
                Assert.IsTrue(existeixRF, "El component 'RF1123' de les dades demo no s'ha trobat.");

                // Comprovem un altre component
                bool existeix73 = components.Any(c => c.Codi == "7363GH"); //
                Assert.IsTrue(existeix73, "El component '7363GH' de les dades demo no s'ha trobat.");
            }
            // (No cal Commit, només hem llegit)
        }
    }
}
