using Dapper;
using MvvmLight4.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Service
{
    public class InitService : IInitService
    {
        private static InitService initService = new InitService();
        private InitService() { }
        public static InitService GetService()
        {
            return initService;
        }

        /// <summary>
        /// 主页面
        /// 初始化数据库
        /// </summary>
        /// <param name="sqlFile"></param>
        /// <returns></returns>
        public int InitDatabase(string sqlFile)
        {
            SQLiteConnection.CreateFile(sqlFile);
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                string script = File.ReadAllText("Util/main.sql");
                int result = conn.Execute(script);
                return result;
            }
        }
    }
}
