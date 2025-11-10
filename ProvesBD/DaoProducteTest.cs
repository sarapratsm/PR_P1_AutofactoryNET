using DAO;
using Model;
using System.Collections.ObjectModel;
using Oracle.EntityFrameworkCore;

namespace ProvesBD
{
    [TestClass]
    public sealed class DaoProducteTest
    {

        [TestInitialize]
        public void TestInitialize()
        {
            // Aquí pots posar codi que s'executi abans de cada test, si cal.
            // Per exemple, podries reiniciar l'estat de la base de dades.
        }

        /// <summary>
        /// Comprova que 'ObtenirTotsProductes' retorna les dades
        /// de demo inserides amb l'script SQL.
        /// </summary>
        [TestMethod]
        public void Test_Lectura_ObtenirTotsProductes_RetornaDadesDemo()
        {
            // Arrange
            using (UnitOfWork uow = OracleFactory.getUOW())
            {
                IDAOProducte dao = uow.DAOProducte;
                List<Producte> productes = null;

                // Act
                try
                {
                    productes = dao.ObtenirTots();
                }
                catch (System.Exception ex)
                {
                    Assert.Fail($"L'execució de ObtenirTotsProductes ha fallat: {ex.Message}");
                }

                // Assert
                Assert.IsNotNull(productes, "La llista de productes no hauria de ser nul·la.");

                // Comprovem que hi ha 4 productes, segons el DadesDemo.sql
                Assert.IsTrue(productes.Count >= 4, "S'esperaven almenys 4 productes de les dades demo.");

                // Comprovem un producte específic
                bool existeix = productes.Any(p => p.Codi == "SFD_CR_01"); //
                Assert.IsTrue(existeix, "El producte 'SFD_CR_01' de les dades demo no s'ha trobat.");
            }
        }
        /*
        [TestMethod]
        public void ProvaSelect()
        {
            UnitOfWork uow = OracleFactory.getUOW();
            
            
            ObservableCollection<Producte> productes = uow.DAOProducte.GetProductes("", "");

            Assert.IsNotNull(productes);
            Assert.AreEqual(1, productes.Count()); //el primer parametre ha de ser el count a la base de dades real
            
        }*/

    }
}
