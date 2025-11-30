using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.Common;
using System.Data; // Per a DbType

namespace DAO
{
    public static class Utils
    {

        public static void CrearParametres(DbCommand consulta, object value, string name, DbType tipus)
        {
            DbParameter p = consulta.CreateParameter();
            p.Value = value ?? DBNull.Value; // Assigna DBNull si el valor és nul
            p.ParameterName = name;
            p.DbType = tipus;
            consulta.Parameters.Add(p);
        }

       
    }//end Utils

}//end namespace Persistencia