using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLight4.Common;
using Dapper;
using MvvmLight4.Model;

namespace MvvmLight4.Service
{
    class ExportService : IExportService
    {
        private static ExportService exportService = new ExportService();
        private ExportService() { }
        public static ExportService GetService() { return exportService; }

        public ObservableCollection<ExportModel> SelectAll()
        {
            ObservableCollection<ExportModel> ems = new ObservableCollection<ExportModel>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT * FROM TB_EXPORT;";
                IEnumerable<dynamic> dynamics = conn.Query(sql);
                foreach (var item in dynamics)
                {
                    ExportModel em = new ExportModel();
                    em.Alternative = item.ALTERNATIVE;
                    em.Byname = item.BYNAME;
                    em.IsChoose = (int)item.ISCHOOSE;
                    ems.Add(em);
                }
            }
            return ems;
        }

        public Dictionary<string, string> SelectChoose()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT ALTERNATIVE,BYNAME FROM TB_EXPORT WHERE ISCHOOSE = 1;";
                IEnumerable<dynamic> dynamics = conn.Query(sql);
                foreach (var item in dynamics)
                {
                    keyValuePairs.Add(item.ALTERNATIVE, item.BYNAME);
                }
            }
            return keyValuePairs;
        }

        public void UpdateExport(ObservableCollection<ExportModel> exports)
        {
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"UPDATE TB_EXPORT SET BYNAME=@Byname,ISCHOOSE=@IsChoose WHERE ALTERNATIVE=@Alternative;";
                foreach (var item in exports)
                {
                    conn.Execute(sql, new
                    {
                        item.Byname,
                        item.IsChoose,
                        item.Alternative
                    });
                }
            }
        }

        public List<ExportModel> SelectChooseToList()
        {
            List<ExportModel> exportModels = new List<ExportModel>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT ALTERNATIVE,BYNAME,ISCHOOSE FROM TB_EXPORT WHERE ISCHOOSE = 1;";
                exportModels = conn.Query<ExportModel>(sql).ToList();
            }
            return exportModels;
        }
    }
}
