using DAO;
using Microsoft.EntityFrameworkCore; // Per a .Database.GetDbConnection()
using Microsoft.EntityFrameworkCore.Storage;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;           // Per a DbConnection, DbCommand, DbDataReader
using System.Data.Common;
using System.IO;
using System.Text;
using System.Data; // Per a DbType

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
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    // Enllaça la comanda amb la transacció del UoW
                    cmd.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 1. Crear l'objecte "net" (Provincia)
                            Provincia provincia = null;
                            if (reader["pr_codi"] != DBNull.Value)
                            {
                                provincia = new Provincia(
                                    Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pr_codi"))),
                                    reader.GetString(reader.GetOrdinal("pr_nom"))
                                );
                            }

                            // 2. Crear l'objecte dependent (Municipi)
                            Municipi municipi = null;
                            if (reader["mu_codi"] != DBNull.Value)
                            {
                                municipi = new Municipi(
                                    Convert.ToInt32(reader.GetValue(reader.GetOrdinal("mu_codi"))),
                                    reader.GetString(reader.GetOrdinal("mu_nom")),
                                    provincia
                                );
                            }

                            // 3. Crear l'objecte principal (Proveidor)
                            Proveidor proveidor = new Proveidor(
                                Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pv_codi"))),
                                reader.GetString(reader.GetOrdinal("pv_CIF")),
                                reader.GetString(reader.GetOrdinal("pv_rao_social")),
                                reader.GetString(reader.GetOrdinal("pv_linia_adreça_facturacio")),
                                reader.IsDBNull(reader.GetOrdinal("pv_persona_contacte")) ? null : reader.GetString(reader.GetOrdinal("pv_persona_contacte")),
                                reader.IsDBNull(reader.GetOrdinal("pv_telf_contacte")) ? null : reader.GetString(reader.GetOrdinal("pv_telf_contacte")),
                                municipi
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
            // Patró basat en InsertClient
            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != ConnectionState.Open)
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
                using (var cmd = connexio.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO PROVEIDOR (
                                            pv_codi, pv_CIF, pv_rao_social, pv_linia_adreça_facturacio,
                                            pv_persona_contacte, pv_telf_contacte,
                                            pv_mu_pr_codi, pv_mu_codi
                                        ) VALUES (
                                            :cod, :cif, :rao, :adr, :contacte, :telf, :muni_prov_codi, :muni_codi
                                        )";

                    cmd.Transaction = transaccio.GetDbTransaction();

                    // En el teu DDL, el codi de proveïdor NO és autoincremental
                    // (igual que a les dades demo),
                    // per tant, l'hem de passar des de l'objecte 'p'.
                    Utils.CrearParametres(cmd, p.Codi, ":cod", DbType.Int32);
                    Utils.CrearParametres(cmd, p.Cif, ":cif", DbType.String);
                    Utils.CrearParametres(cmd, p.RaoSocial, ":rao", DbType.String);
                    Utils.CrearParametres(cmd, p.AdrFacturacio, ":adr", DbType.String);
                    Utils.CrearParametres(cmd, p.Persona_contacte, ":contacte", DbType.String);
                    Utils.CrearParametres(cmd, p.TelfContacte, ":telf", DbType.String);

                    // Gestió de claus foranes nul·les
                    if (p.Municipi != null)
                    {
                        Utils.CrearParametres(cmd, p.Municipi.ProvinciaCodi, ":muni_prov_codi", DbType.Int32);
                        Utils.CrearParametres(cmd, p.Municipi.Codi, ":muni_codi", DbType.Int32);
                    }
                    else
                    {
                        Utils.CrearParametres(cmd, null, ":muni_prov_codi", DbType.Int32);
                        Utils.CrearParametres(cmd, null, ":muni_codi", DbType.Int32);
                    }

                    int filesAfectades = cmd.ExecuteNonQuery();

                    if (filesAfectades == 1)
                    {
                        return true;
                    }
                    else
                    {
                        transaccio.Rollback();
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                transaccio.Rollback();
                throw;
            }
        }

        public Proveidor ObtenirProveidor(int codi_prov)
        {
            // Patró basat en GetProvincia
            Proveidor proveidor = null;

            string sql = @"SELECT 
                               pv.pv_codi, pv.pv_CIF, pv.pv_rao_social, pv.pv_linia_adreça_facturacio, 
                               pv.pv_persona_contacte, pv.pv_telf_contacte,
                               m.mu_codi, m.mu_nom,
                               pr.pr_codi, pr.pr_nom
                           FROM PROVEIDOR pv
                           LEFT JOIN MUNICIPI m ON pv.pv_mu_pr_codi = m.mu_pr_codi AND pv.pv_mu_codi = m.mu_codi
                           LEFT JOIN PROVINCIA pr ON m.mu_pr_codi = pr.pr_codi
                           WHERE pv.pv_codi = :id"; // <-- Afegim el WHERE

            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != ConnectionState.Open)
            {
                connexio.Open();
            }

            using (DbCommand cmd = connexio.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();

                Utils.CrearParametres(cmd, codi_prov, ":id", DbType.Int32);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) // Només esperem un resultat
                    {
                        // (Mateixa lògica de construcció que a ObtenirTots)
                        Provincia provincia = null;
                        if (reader["pr_codi"] != DBNull.Value)
                        {
                            provincia = new Provincia(
                                Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pr_codi"))),
                                reader.GetString(reader.GetOrdinal("pr_nom"))
                            );
                        }

                        Municipi municipi = null;
                        if (reader["mu_codi"] != DBNull.Value)
                        {
                            municipi = new Municipi(
                                Convert.ToInt32(reader.GetValue(reader.GetOrdinal("mu_codi"))),
                                reader.GetString(reader.GetOrdinal("mu_nom")),
                                provincia
                            );
                        }

                        proveidor = new Proveidor(
                            Convert.ToInt32(reader.GetValue(reader.GetOrdinal("pv_codi"))),
                            reader.GetString(reader.GetOrdinal("pv_CIF")),
                            reader.GetString(reader.GetOrdinal("pv_rao_social")),
                            reader.GetString(reader.GetOrdinal("pv_linia_adreça_facturacio")),
                            reader.IsDBNull(reader.GetOrdinal("pv_persona_contacte")) ? null : reader.GetString(reader.GetOrdinal("pv_persona_contacte")),
                            reader.IsDBNull(reader.GetOrdinal("pv_telf_contacte")) ? null : reader.GetString(reader.GetOrdinal("pv_telf_contacte")),
                            municipi
                        );
                    }
                }
            }
            return proveidor; // Retorna l'objecte o null si no s'ha trobat
        }

        public bool ModificarProveidor(Proveidor p)
        {
            throw new NotImplementedException();
        }

        public bool EliminarProveidor(int codi_prov)
        {
            // Patró basat en DeleteClient
            var connexio = _context.Database.GetDbConnection();
            if (connexio.State != ConnectionState.Open)
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
                DbParameter paramCodi = null;

                // 1. Esborrar de PROV_COMP primer (per FK)
                using (var cmd = connexio.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM PROV_COMP WHERE pc_pv_codi = :cod";
                    cmd.Transaction = transaccio.GetDbTransaction();
                    paramCodi = cmd.CreateParameter();
                    paramCodi.ParameterName = ":cod";
                    paramCodi.Value = codi_prov;
                    cmd.Parameters.Add(paramCodi);
                    cmd.ExecuteNonQuery(); // Pot ser 0
                }

                // 2. Esborrar de PROVEIDOR
                using (var cmd = connexio.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM PROVEIDOR WHERE pv_codi = :cod";
                    cmd.Transaction = transaccio.GetDbTransaction();
                    paramCodi = cmd.CreateParameter();
                    paramCodi.ParameterName = ":cod";
                    paramCodi.Value = codi_prov;
                    cmd.Parameters.Add(paramCodi);
                    filesAfectades = cmd.ExecuteNonQuery();
                }

                if (filesAfectades == 1)
                {
                    return true;
                }
                else
                {
                    transaccio.Rollback();
                    return false; // No s'ha trobat el proveïdor
                }
            }
            catch (Exception)
            {
                transaccio.Rollback();
                throw;
            }
        }

        public Dictionary<string, decimal> ObternirProvsPreu(Componente c)
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


    }//end DAOProveidor

}//end namespace Persistencia