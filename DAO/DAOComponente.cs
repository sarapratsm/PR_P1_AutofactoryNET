
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;


namespace DAO
{
    public class DAOComponente : IDAOComponente
    {
        // Dependència del DbContext
        private readonly MyDBContext _context;

        //Constructor
        public DAOComponente(MyDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }



        public bool AfegirComponent(Componente component)
        {
            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != System.Data.ConnectionState.Open)
            {
                connexio.Open();
            }
            var transaccio = _context.Database.CurrentTransaction;
            if (transaccio == null)
            {
                throw new InvalidOperationException("No hi ha cap transacció activa. Crida a BeginTransaction() des del UnitOfWork.");
            }

            try
            {
                int filesAfectades = 0;

                // SQL 1: Inserir a la taula ITEM
                using (var cmdItem = connexio.CreateCommand())
                {
                    string sqlItem = @"INSERT INTO ITEM (it_codi, it_tipus, it_nom, it_desc, it_stock, it_foto) 
                                       VALUES (:cod, 'C', :nom, :descr, :stock, :foto)";
                    cmdItem.CommandText = sqlItem;
                    cmdItem.Transaction = transaccio.GetDbTransaction();

                    Utils.CrearParametres(cmdItem, component.Codi, ":cod", DbType.String);
                    Utils.CrearParametres(cmdItem, component.Nom, ":nom", DbType.String);
                    Utils.CrearParametres(cmdItem, component.Descripcio, ":descr", DbType.String);
                    Utils.CrearParametres(cmdItem, component.Stock, ":stock", DbType.Int32);
                    Utils.CrearParametres(cmdItem, component.Foto, ":foto", DbType.Binary);

                    filesAfectades = cmdItem.ExecuteNonQuery();
                }

                // SQL 2: Inserir a la taula COMPONENTE
                if (filesAfectades > 0)
                {
                    using (var cmdComp = connexio.CreateCommand())
                    {
                        // El preu mig (cm_preu_mig) no cal inserir-lo,
                        // ja que el teu trigger t_comp_before el posarà a 0.
                        string sqlComp = @"INSERT INTO COMPONENTE (cm_it_codi, cm_um_codi, cm_codi_fabricant) 
                                           VALUES (:cod, :um_codi, :codi_fab)";
                        cmdComp.CommandText = sqlComp;
                        cmdComp.Transaction = transaccio.GetDbTransaction();

                        Utils.CrearParametres(cmdComp, component.Codi, ":cod", DbType.String);
                        Utils.CrearParametres(cmdComp, (int)component.Unitat, ":um_codi", DbType.Int32);
                        Utils.CrearParametres(cmdComp, component.CodiFabricant, ":codi_fab", DbType.String);

                        filesAfectades += cmdComp.ExecuteNonQuery();
                    }
                }

                if (filesAfectades == 2) // Ha d'haver inserit a ITEM i a COMPONENTE
                {
                    return true;
                }
                else
                {
                    transaccio.Rollback();
                    return false;
                }
            }
            catch (Exception)
            {
                transaccio.Rollback();
                throw;
            }
        }

        public bool ModificarComponent(Componente component)
        {
            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != System.Data.ConnectionState.Open)
            {
                connexio.Open();
            }
            var transaccio = _context.Database.CurrentTransaction;
            if (transaccio == null)
            {
                throw new InvalidOperationException("No hi ha cap transacció activa. Crida a BeginTransaction() des del UnitOfWork.");
            }

            try
            {
                int filesAfectades = 0;

                // SQL 1: Actualitzar ITEM
                using (var cmdItem = connexio.CreateCommand())
                {
                    string sqlItem = @"UPDATE ITEM SET
                                           it_nom = :nom,
                                           it_desc = :descr,
                                           it_stock = :stock,
                                           it_foto = :foto
                                       WHERE it_codi = :cod";
                    cmdItem.CommandText = sqlItem;
                    cmdItem.Transaction = transaccio.GetDbTransaction();

                    Utils.CrearParametres(cmdItem, component.Nom, ":nom", DbType.String);
                    Utils.CrearParametres(cmdItem, component.Descripcio, ":descr", DbType.String);
                    Utils.CrearParametres(cmdItem, component.Stock, ":stock", DbType.Int32);
                    Utils.CrearParametres(cmdItem, component.Foto, ":foto", DbType.Binary);
                    Utils.CrearParametres(cmdItem, component.Codi, ":cod", DbType.String);

                    filesAfectades = cmdItem.ExecuteNonQuery();
                }

                // SQL 2: Actualitzar COMPONENTE
                if (filesAfectades > 0)
                {
                    using (var cmdComp = connexio.CreateCommand())
                    {
                        // No actualitzem el preu mig, el trigger ho prohibeix
                        string sqlComp = @"UPDATE COMPONENTE SET
                                               cm_um_codi = :um_codi,
                                               cm_codi_fabricant = :codi_fab
                                           WHERE cm_it_codi = :cod";
                        cmdComp.CommandText = sqlComp;
                        cmdComp.Transaction = transaccio.GetDbTransaction();

                        Utils.CrearParametres(cmdComp, (int)component.Unitat, ":um_codi", DbType.Int32);
                        Utils.CrearParametres(cmdComp, component.CodiFabricant, ":codi_fab", DbType.String);
                        Utils.CrearParametres(cmdComp, component.Codi, ":cod", DbType.String);

                        filesAfectades += cmdComp.ExecuteNonQuery();
                    }
                }

                // Comprovem que s'han actualitzat les DUES taules
                if (filesAfectades == 2)
                {
                    return true;
                }
                else
                {
                    // Si no s'ha trobat (filesAfectades = 0) o només ha actualitzat 1, desfem
                    transaccio.Rollback();
                    return false;
                }
            }
            catch (Exception)
            {
                transaccio.Rollback();
                throw;
            }
        }

        public bool EliminarComponent(string codi)
        {
            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != System.Data.ConnectionState.Open)
            {
                connexio.Open();
            }
            var transaccio = _context.Database.CurrentTransaction;
            if (transaccio == null)
            {
                throw new InvalidOperationException("No hi ha cap transacció activa. Crida a BeginTransaction() des del UnitOfWork.");
            }

            try
            {
                int filesAfectades = 0;
                DbParameter paramCodi = null; // Per reutilitzar el paràmetre

                // 1. Esborrar de PROV_COMP (relació proveïdors)
                using (var cmd = connexio.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM PROV_COMP WHERE pc_cm_it_codi = :cod";
                    cmd.Transaction = transaccio.GetDbTransaction();
                    paramCodi = cmd.CreateParameter();
                    paramCodi.ParameterName = ":cod";
                    paramCodi.Value = codi;
                    cmd.Parameters.Add(paramCodi);
                    cmd.ExecuteNonQuery(); // No sumem files, pot ser 0
                }

                // 2. Esborrar de PROD_ITEM (on és usat per productes)
                using (var cmd = connexio.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM PROD_ITEM WHERE pi_it_codi = :cod";
                    cmd.Transaction = transaccio.GetDbTransaction();
                    paramCodi = cmd.CreateParameter();
                    paramCodi.ParameterName = ":cod";
                    paramCodi.Value = codi;
                    cmd.Parameters.Add(paramCodi);
                    cmd.ExecuteNonQuery();
                }

                // 3. Esborrar de COMPONENTE (la fila filla)
                using (var cmd = connexio.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM COMPONENTE WHERE cm_it_codi = :cod";
                    cmd.Transaction = transaccio.GetDbTransaction();
                    paramCodi = cmd.CreateParameter();
                    paramCodi.ParameterName = ":cod";
                    paramCodi.Value = codi;
                    cmd.Parameters.Add(paramCodi);
                    filesAfectades = cmd.ExecuteNonQuery();
                }

                // 4. Esborrar de ITEM (la fila pare)
                if (filesAfectades > 0)
                {
                    using (var cmd = connexio.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM ITEM WHERE it_codi = :cod";
                        cmd.Transaction = transaccio.GetDbTransaction();
                        paramCodi = cmd.CreateParameter();
                        paramCodi.ParameterName = ":cod";
                        paramCodi.Value = codi;
                        cmd.Parameters.Add(paramCodi);
                        filesAfectades += cmd.ExecuteNonQuery();
                    }
                }

                if (filesAfectades == 2) // Ha d'haver esborrat de COMPONENTE i de ITEM
                {
                    return true;
                }
                else
                {
                    // Si no s'ha trobat (filesAfectades = 0) o només ha esborrat 1, desfem
                    transaccio.Rollback();
                    return false;
                }
            }
            catch (Exception)
            {
                transaccio.Rollback();
                throw;
            }
        }

        /*public ObservableCollection<Componente> ObtenirComponentsFiltrats(string codi_filtre, string nom_filtre)
        {
            ObservableCollection<Componente> components = new ObservableCollection<Componente>();
            var connexio = _context.Database.GetDbConnection();

            if (connexio.State != ConnectionState.Open)
            {  
                connexio.Open();
            }



            using (DbCommand consulta = connexio.CreateCommand())
            {
                string sql = @"SELECT 
                           i.it_codi, i.it_nom, i.it_desc, i.it_stock, i.it_foto, i.it_tipus,
                           c.cm_codi_fabricant, c.cm_um_codi, c.cm_preu_mig
                       FROM ITEM i
                       JOIN COMPONENTE c ON i.it_codi = c.cm_it_codi
                       WHERE i.it_tipus = 'C' 
                         AND (:COD_FILTRE = '%%' OR UPPER(i.it_codi) LIKE :COD_FILTRE)
                         AND (:NOM_FILTRE = '%%' OR UPPER(i.it_nom) LIKE :NOM_FILTRE)";

                consulta.CommandText = sql;
                consulta.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction(); // IMPORTANT !!!

                string codi_param = "%" + (codi_filtre ?? "").ToUpper() + "%";
                string nom_param = "%" + (nom_filtre ?? "").ToUpper() + "%";
                Utils.CrearParametres(consulta, codi_param, "COD_FILTRE", DbType.String);
                Utils.CrearParametres(consulta, nom_param, "NOM_FILTRE", DbType.String);

                


                using (DbDataReader reader = consulta.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Descarreguem les dades de cada columna de la taula
                        string codi = reader.GetString(reader.GetOrdinal("it_codi"));
                        string nom = reader.GetString(reader.GetOrdinal("it_nom"));
                        string descripcio = reader.IsDBNull(reader.GetOrdinal("it_desc")) ? null : reader.GetString(reader.GetOrdinal("it_desc"));
                        int stock = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("it_stock")));
                        byte[] fotoBytes = null;
                        if (reader["it_foto"] != DBNull.Value)
                        {
                            fotoBytes = (byte[])reader["it_foto"];
                        }
                        char tipus = reader.GetString(reader.GetOrdinal("it_tipus"))[0];
                        string codiFabricant = reader.GetString(reader.GetOrdinal("cm_codi_fabricant"));
                        TipusUnitat unitat = (TipusUnitat)Convert.ToInt32(reader.GetValue(reader.GetOrdinal("cm_um_codi")));
                        //double preu_mig; //Es calcularà automàticament
                        Componente c = new Componente(codi, nom, descripcio, stock, fotoBytes, tipus, codiFabricant, unitat);
                        components.Add(c);
                    }
                }
            }
            return components;
        }*/

        public ObservableCollection<Componente> ObtenirComponentsFiltrats(string codi_filtre, string nom_filtre, int offset = 0, int rows_per_page = -1)
        {
            ObservableCollection<Componente> components = new ObservableCollection<Componente>();
            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != ConnectionState.Open)
            {
                connexio.Open();
            }

            using (DbCommand consulta = connexio.CreateCommand())
            {
                // SQL SIMPLIFICAT: Eliminem la part (:VAR = '%%') per evitar duplicar paràmetres
                string sql = @"SELECT 
                   i.it_codi, i.it_nom, i.it_desc, i.it_stock, i.it_foto, i.it_tipus,
                   c.cm_codi_fabricant, c.cm_um_codi, c.cm_preu_mig
               FROM ITEM i
               JOIN COMPONENTE c ON i.it_codi = c.cm_it_codi
               WHERE i.it_tipus = 'C' 
                 AND UPPER(i.it_codi) LIKE :COD_FILTRE
                 AND UPPER(i.it_nom) LIKE :NOM_FILTRE";

                if (rows_per_page > 0)
                {
                    consulta.CommandText += $" limit {rows_per_page} offset {offset}";
                }

                consulta.CommandText = sql;
                consulta.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

                // Preparem els paràmetres amb '%' inclòs
                // Si el filtre és buit, posem '%' per a que ho trobi tot
                string codi_param = string.IsNullOrEmpty(codi_filtre) ? "%" : "%" + codi_filtre.ToUpper() + "%";
                string nom_param = string.IsNullOrEmpty(nom_filtre) ? "%" : "%" + nom_filtre.ToUpper() + "%";

                // Creem els paràmetres SENSE els dos punts (correcte per a OracleClient)
                Utils.CrearParametres(consulta, codi_param, "COD_FILTRE", DbType.String);
                Utils.CrearParametres(consulta, nom_param, "NOM_FILTRE", DbType.String);

                using (DbDataReader reader = consulta.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // (El teu codi de lectura original)
                        string codi = reader.GetString(reader.GetOrdinal("it_codi"));
                        string nom = reader.GetString(reader.GetOrdinal("it_nom"));
                        string descripcio = reader.IsDBNull(reader.GetOrdinal("it_desc")) ? null : reader.GetString(reader.GetOrdinal("it_desc"));
                        int stock = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("it_stock")));

                        byte[] fotoBytes = null;
                        if (reader["it_foto"] != DBNull.Value)
                        {
                            fotoBytes = (byte[])reader["it_foto"];
                        }

                        char tipus = reader.GetString(reader.GetOrdinal("it_tipus"))[0];
                        string codiFabricant = reader.GetString(reader.GetOrdinal("cm_codi_fabricant"));
                        TipusUnitat unitat = (TipusUnitat)Convert.ToInt32(reader.GetValue(reader.GetOrdinal("cm_um_codi")));

                        Componente c = new Componente(codi, nom, descripcio, stock, fotoBytes, tipus, codiFabricant, unitat);
                        components.Add(c);
                    }
                }
            }
            return components;
        }

        public Dictionary<string, decimal> ObternirProvCodPreu(Componente c)
        {
            Dictionary<string, decimal> proveidorsPreu = new Dictionary<string, decimal>();

            // SQL per obtenir NOM PROVEÏDOR i PREU
            string sql = @"SELECT 
                    pv.pv_rao_social, 
                    pc.pc_preu
                   FROM PROV_COMP pc
                   JOIN PROVEIDOR pv ON pc.pc_pv_codi = pv.pv_codi
                   WHERE pc.pc_cm_it_codi = :id";

            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != ConnectionState.Open)
            {
                connexio.Open();
            }

            using (DbCommand cmd = connexio.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

                Utils.CrearParametres(cmd, c.Codi, ":id", DbType.String);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string nomProveidor = reader.GetString(reader.GetOrdinal("pv_rao_social"));
                        decimal preu = reader.GetDecimal(reader.GetOrdinal("pc_preu"));

                        if (!proveidorsPreu.ContainsKey(nomProveidor))
                        {
                            proveidorsPreu.Add(nomProveidor, preu);
                        }
                    }
                }
            }
            return proveidorsPreu;
        }

        public Componente ObtenirComponentPerCodi(string codi)
        {
            Componente component = null;

            string sql = @"SELECT 
                               i.it_codi, i.it_nom, i.it_desc, i.it_stock, i.it_foto, i.it_tipus,
                               c.cm_codi_fabricant, c.cm_um_codi, c.cm_preu_mig
                           FROM ITEM i
                           JOIN COMPONENTE c ON i.it_codi = c.cm_it_codi
                           WHERE i.it_tipus = 'C' AND i.it_codi = :id";

            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != ConnectionState.Open)
            {
                connexio.Open();
            }

            using (DbCommand cmd = connexio.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

                Utils.CrearParametres(cmd, codi, ":id", DbType.String);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string nom = reader.GetString(reader.GetOrdinal("it_nom"));
                        string descripcio = reader.IsDBNull(reader.GetOrdinal("it_desc")) ? null : reader.GetString(reader.GetOrdinal("it_desc"));
                        int stock = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("it_stock")));

                        byte[] fotoBytes = null;
                        if (reader["it_foto"] != DBNull.Value)
                        {
                            fotoBytes = (byte[])reader["it_foto"];
                        }

                        char tipus = reader.GetString(reader.GetOrdinal("it_tipus"))[0];
                        string codiFabricant = reader.GetString(reader.GetOrdinal("cm_codi_fabricant"));
                        TipusUnitat unitat = (TipusUnitat)Convert.ToInt32(reader.GetValue(reader.GetOrdinal("cm_um_codi")));

                        // Creem el component
                        component = new Componente(codi, nom, descripcio, stock, fotoBytes, tipus, codiFabricant, unitat);

                        
                    }
                }
            }
            return component;
        }

        public Dictionary<Proveidor, decimal> ObtenirProveidorsPreuFiltrats(string codi_comp, string cif_filtre, string raoSocial_filtre, int offset = 0, int rows_per_page = -1)
        {
            Dictionary<Proveidor, decimal> proveidorsPreu = new Dictionary<Proveidor, decimal>();

            

            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != ConnectionState.Open)
            {
                connexio.Open();
            }

            using (DbCommand consulta = connexio.CreateCommand())
            {
                // SQL que fa JOIN entre PROV_COMP i PROVEIDOR (i les seves taules)
                // per obtenir l'objecte Proveidor complet i el preu
                string sql = @"SELECT 
                               pv.pv_codi, pv.pv_CIF, pv.pv_rao_social, pv.pv_linia_adreça_facturacio, 
                               pv.pv_persona_contacte, pv.pv_telf_contacte,
                               m.mu_codi, m.mu_nom,
                               pr.pr_codi, pr.pr_nom,
                               pc.pc_preu  -- El preu d'aquest component per a aquest proveïdor
                           FROM PROV_COMP pc
                           JOIN PROVEIDOR pv ON pc.pc_pv_codi = pv.pv_codi
                           LEFT JOIN MUNICIPI m ON pv.pv_mu_pr_codi = m.mu_pr_codi AND pv.pv_mu_codi = m.mu_codi
                           LEFT JOIN PROVINCIA pr ON m.mu_pr_codi = pr.pr_codi
                           WHERE pc.pc_cm_it_codi = :cod_comp
                                AND UPPER(pv.pv_cif) LIKE :CIF_FILTRE
                                AND UPPER(pv.pv_rao_social) LIKE :RS_FILTRE";

                if (rows_per_page > 0)
                {
                    consulta.CommandText += $" limit {rows_per_page} offset {offset}";
                }

                consulta.CommandText = sql;
                consulta.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

                Utils.CrearParametres(consulta, codi_comp, ":cod_comp", DbType.String);
                // Preparem els paràmetres amb '%' inclòs
                // Si el filtre és buit, posem '%' per a que ho trobi tot
                string cif_param = string.IsNullOrEmpty(cif_filtre) ? "%" : "%" + cif_filtre.ToUpper() + "%";
                string rs_param = string.IsNullOrEmpty(raoSocial_filtre) ? "%" : "%" + raoSocial_filtre.ToUpper() + "%";
                Utils.CrearParametres(consulta, cif_param, ":CIF_FILTRE", DbType.String);
                Utils.CrearParametres(consulta, rs_param, ":RS_FILTRE", DbType.String);


                using (DbDataReader reader = consulta.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // 1. Crear Provincia (si existeix)
                        Provincia provincia = null;
                        if (reader["pr_codi"] != DBNull.Value)
                        {
                            provincia = new Provincia(
                                Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pr_codi"))),
                                reader.GetString(reader.GetOrdinal("pr_nom"))
                            );
                        }

                        // 2. Crear Municipi (si existeix)
                        Municipi municipi = null;
                        if (reader["mu_codi"] != DBNull.Value)
                        {
                            municipi = new Municipi(
                                Convert.ToInt32(reader.GetValue(reader.GetOrdinal("mu_codi"))),
                                reader.GetString(reader.GetOrdinal("mu_nom")),
                                provincia
                            );
                        }

                        // 3. Crear Proveidor
                        Proveidor proveidor = new Proveidor(
                            Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pv_codi"))),
                            reader.GetString(reader.GetOrdinal("pv_CIF")),
                            reader.GetString(reader.GetOrdinal("pv_rao_social")),
                            reader.GetString(reader.GetOrdinal("pv_linia_adreça_facturacio")),
                            reader.IsDBNull(reader.GetOrdinal("pv_persona_contacte")) ? null : reader.GetString(reader.GetOrdinal("pv_persona_contacte")),
                            reader.IsDBNull(reader.GetOrdinal("pv_telf_contacte")) ? null : reader.GetString(reader.GetOrdinal("pv_telf_contacte")),
                            municipi
                        );

                        // 4. Llegir el preu
                        decimal preu = reader.GetDecimal(reader.GetOrdinal("pc_preu"));

                        // 5. Afegir al diccionari
                        if (!proveidorsPreu.ContainsKey(proveidor))
                        {
                            proveidorsPreu.Add(proveidor, preu);
                        }
                    }
                }
            }
            return proveidorsPreu;
        }


        
        public decimal CalculaPreuMig(Componente component)
       {
           // Cridem la funció d'Oracle f_calcula_preu_mig

           var connexio = _context.Database.GetDbConnection();
           if (connexio.State != ConnectionState.Open)
           {
               connexio.Open();
           }

           using (var cmd = connexio.CreateCommand())
           {
               cmd.CommandText = "SELECT f_calcula_preu_mig(:cod) FROM DUAL";
               cmd.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

               Utils.CrearParametres(cmd, component.Codi, ":cod", DbType.String);

               // ExecuteScalar s'usa per a consultes que retornen un sol valor
               var resultat = cmd.ExecuteScalar();

               if (resultat != null && resultat != DBNull.Value)
               {
                   return Convert.ToDecimal(resultat);
               }
           }
           return 0;
       }
       

        #region metodes que no vull aplicar ara mateix
        /*public ObservableCollection<Componente> ObtenirTotsESBORRAR()
        {
            ObservableCollection<Componente> components = new ObservableCollection<Componente>();

            // Query adaptada al DDL
            string sql = @"SELECT 
                               i.it_codi, i.it_nom, i.it_desc, i.it_stock, i.it_foto, i.it_tipus,
                               c.cm_codi_fabricant, c.cm_um_codi, c.cm_preu_mig
                           FROM ITEM i
                           JOIN COMPONENTE c ON i.it_codi = c.cm_it_codi
                           WHERE i.it_tipus = 'C'"; 

            using (DbConnection conn = _context.Database.GetDbConnection())
            {
                conn.Open();
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;

                    // Enllaça la comanda amb la transacció del UoW
                    cmd.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string codi = reader.GetString(reader.GetOrdinal("it_codi"));
                            string nom = reader.GetString(reader.GetOrdinal("it_nom"));
                            string descripcio = reader.IsDBNull(reader.GetOrdinal("it_desc")) ? null : reader.GetString(reader.GetOrdinal("it_desc"));
                            int stock = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("it_stock")));

                            // NOTA FOTO: DDL és VARCHAR, Model és byte[]
                            // string fotoPath = reader.IsDBNull(reader.GetOrdinal("it_foto")) ? null : reader.GetString(reader.GetOrdinal("it_foto"));
                            byte[] fotoBytes = null; // Passant null al constructor

                            char tipus = reader.GetString(reader.GetOrdinal("it_tipus"))[0];
                            string codiFabricant = reader.GetString(reader.GetOrdinal("cm_codi_fabricant"));
                            TipusUnitat unitat = (TipusUnitat)Convert.ToInt32(reader.GetValue(reader.GetOrdinal("cm_um_codi")));

                            // Creem el component
                            Componente c = new Componente(codi, nom, descripcio, stock, fotoBytes, tipus, codiFabricant, unitat);

                            // TODO: Assignar el preu mig llegit (cm_preu_mig)
                            // El teu model C# no té un setter públic per a PreuMig.
                            // Hauries d'afegir-lo o carregar-lo a través de CalculaPreuMig.

                            components.Add(c);
                        }
                    }
                }
            }
            return components;
        }*/
        #endregion

    }//end DAOComponent

}//end namespace Persistencia