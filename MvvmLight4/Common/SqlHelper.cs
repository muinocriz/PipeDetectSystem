using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Common
{
    public class SqlHelper
    {
        private static string connStr = ConfigurationManager.ConnectionStrings["dbConnStr"].ConnectionString;

        public static IDbConnection GetConnection()
        {
            return new SQLiteConnection(ConfigurationManager.ConnectionStrings["dbConnStr"].ConnectionString);
        }
    }
}
