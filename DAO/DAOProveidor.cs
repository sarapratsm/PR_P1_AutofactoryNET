using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Model;
using DAO;
using Microsoft.EntityFrameworkCore; // Per a .Database.GetDbConnection()
using System.Data.Common;
using System.Collections.ObjectModel;           // Per a DbConnection, DbCommand, DbDataReader

namespace DAO
{
    public class DAOProveidor : IDAOProveidor
    {

        private readonly MyDBContext _context;

        public DAOProveidor(MyDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ObservableCollection<Proveidor> ObtenirTotsProveidors()
        {
            ObservableCollection<Proveidor> proveidors = new ObservableCollection<Proveidor>();

            // Query adaptada al DDL. Fem LEFT JOIN per si un proveïdor
            // no té municipi assignat (és nul).
            string sql = @"SELECT 
                               pv.pv_codi, pv.pv_CIF, pv.pv_rao_social, pv.pv_linia_adreça_facturacio, 
                               pv.pv_persona_contacte, pv.pv_telf_contacte,
                               m.mu_codi, m.mu_nom,
                               pr.pr_codi, pr.pr_nom
                           FROM PROVEIDOR pv
                           LEFT JOIN MUNICIPI m ON pv.pv_mu_pr_codi = m.mu_pr_codi AND pv.pv_mu_codi = m.mu_codi
                           LEFT JOIN PROVINCIA pr ON m.mu_pr_codi = pr.pr_codi";

            using (DbConnection conn = _context.Database.GetDbConnection())
            {
                conn.Open();
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 1. Crear l'objecte "net" (Provincia)
                            //    Comprovem si és nul (per culpa del LEFT JOIN)
                            Provincia provincia = null;
                            if (reader["pr_codi"] != DBNull.Value)
                            {
                                provincia = new Provincia(
                                    Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pr_codi"))),
                                    reader.GetString(reader.GetOrdinal("pr_nom"))
                                );
                            }

                            // 2. Crear l'objecte dependent (Municipi)
                            //    Comprovem si és nul
                            Municipi municipi = null;
                            if (reader["mu_codi"] != DBNull.Value)
                            {
                                municipi = new Municipi(
                                    Convert.ToInt32(reader.GetValue(reader.GetOrdinal("mu_codi"))),
                                    reader.GetString(reader.GetOrdinal("mu_nom")),
                                    provincia // Injectem la província (que pot ser nul·la)
                                );
                            }

                            // 3. Crear l'objecte principal (Proveidor)
                            Proveidor proveidor = new Proveidor(
                                Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pv_codi"))),
                                reader.GetString(reader.GetOrdinal("pv_CIF")),
                                reader.GetString(reader.GetOrdinal("pv_rao_social")),
                                reader.GetString(reader.GetOrdinal("pv_linia_adreça_facturacio")), // El camp que vam afegir
                                reader.IsDBNull(reader.GetOrdinal("pv_persona_contacte")) ? null : reader.GetString(reader.GetOrdinal("pv_persona_contacte")),
                                reader.IsDBNull(reader.GetOrdinal("pv_telf_contacte")) ? null : reader.GetString(reader.GetOrdinal("pv_telf_contacte")),
                                municipi // Injectem el municipi (que pot ser nul)
                            );

                            proveidors.Add(proveidor);
                        }
                    }
                }
            }
            return proveidors;
        }


        public bool AfegirProveidor(Proveidor p)
        {
            throw new NotImplementedException();
        }
        public Proveidor ObtenirProveidor(int codi_prov)
        {
            throw new NotImplementedException();
        }

    }//end DAOProveidor

}//end namespace Persistencia