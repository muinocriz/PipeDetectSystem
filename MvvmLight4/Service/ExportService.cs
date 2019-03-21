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
using MvvmLight4.ViewModel;

namespace MvvmLight4.Service
{
    class ExportService : IExportService
    {
        private static ExportService exportService = new ExportService();
        private ExportService() { }
        public static ExportService GetService() { return exportService; }

        /// <summary>
        /// 导出界面
        /// 获取所有可供输出的异常
        /// 已弃用
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 导出界面
        /// 获取选择的属性
        /// 已弃用
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 导出界面
        /// 更新输出属性的选择状态
        /// 已弃用
        /// </summary>
        /// <param name="exports"></param>
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

        /// <summary>
        /// 导出界面
        /// 将选择的属性输出为List
        /// 已弃用
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 导出界面
        /// 根据选择的列表，查到
        /// 输出辅助VM类
        /// --元信息
        /// --每个元信息对应的异常列表
        /// </summary>
        /// <param name="l">导出任务编号列表</param>
        /// <returns></returns>
        public List<ExportData> GetExportListData(List<ExportMeta> l)
        {
            List<ExportData> exportDatas = new List<ExportData>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                foreach (var item in l)
                {
                    ExportData exportData = new ExportData();

                    MetaModel meta = new MetaModel();
                    var sqlMeta = @"SELECT * FROM TB_METADATA WHERE ID=@VideoId";
                    meta = conn.Query<MetaModel>(sqlMeta, new { item.VideoId }).SingleOrDefault();
                    exportData.Meta = meta;

                    List<AbnormalModel> abnormalModels = new List<AbnormalModel>();
                    var sqlAbn = @"SELECT * FROM TB_ABNORMAL WHERE VideoId=@VideoId";
                    abnormalModels = conn.Query<AbnormalModel>(sqlAbn, new { item.VideoId }).ToList();
                    exportData.AbnormalModels = abnormalModels;
                    exportDatas.Add(exportData);
                }
            }
            return exportDatas;
        }
    }
}
