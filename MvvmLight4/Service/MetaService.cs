using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MvvmLight4.Common;
using MvvmLight4.Model;
using MvvmLight4.ViewModel;

namespace MvvmLight4.Service
{
    class MetaService : IMetaService
    {
        private static MetaService metaService = new MetaService();
        private MetaService() { }
        public static MetaService GetService()
        {
            return metaService;
        }

        public int HasVideoPath(string path)
        {
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT COUNT(*) AS COUNT FROM TB_METADATA WHERE VIDEOPATH==@path;";
                int Count = Convert.ToInt32(conn.Query(sql, new { path = path }).FirstOrDefault().COUNT);
                return Count;
            }
        }

        public int InsertData(MetaModel meta)
        {
            int insertedRows = 0;
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                insertedRows = conn.Execute(@"INSERT INTO TB_METADATA ( PIPECODE,
                                                                        PIPETYPE,
                                                                        TASKCODE,
                                                                        ADDR,
                                                                        CHARGE,
                                                                        STARTTIME,
                                                                        ENDTIME,
                                                                        VIDEOPATH) VALUES(
                                                                        @PIPECODE,
                                                                        @PIPETYPE,
                                                                        @TASKCODE,
                                                                        @ADDR,
                                                                        @CHARGE,
                                                                        @STARTTIME,
                                                                        @ENDTIME,
                                                                        @VIDEOPATH)",
                    new
                    {
                        PIPECODE = meta.PipeCode,
                        PIPETYPE = meta.PipeType,
                        TASKCODE =  meta.TaskCode,
                        ADDR = meta.Address,
                        CHARGE = meta.Charge,
                        STARTTIME = meta.StartTime,
                        ENDTIME = meta.EndTime,
                        VIDEOPATH = meta.VideoPath
                    });
            }
            return insertedRows;
        }
        /// <summary>
        /// 标注视频选择窗口，根据选择的视频ID返回分帧之后文件所在位置
        /// </summary>
        /// <param name="id">视频ID</param>
        /// <returns>分帧之后文件所在位置</returns>
        public string QueryFramePathById(int id)
        {
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT DISTINCT FRAMEPATH FROM TB_METADATA WHERE ID = @ID;";
                string framePath = Convert.ToString(conn.Query(sql, new { ID = id }).FirstOrDefault().FRAMEPATH) ;
                return framePath;
            }
        }

        /// <summary>
        /// 获取已分帧的视频列表
        /// </summary>
        /// <returns></returns>

        public List<ComplexInfoModel> QueryVideoFramed()
        {
            List<ComplexInfoModel> l = new List<ComplexInfoModel>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT ID,TASKCODE FROM TB_METADATA WHERE FRAMEPATH IS NOT NULL ORDER BY ID DESC;";
                IEnumerable<dynamic> dynamics = conn.Query(sql);
                foreach (var item in dynamics)
                {
                    ComplexInfoModel c = new ComplexInfoModel();
                    c.Key = Convert.ToString(item.ID);
                    c.Text = item.TASKCODE;
                    l.Add(c);
                }
            }
            return l;
        }

        public ObservableCollection<MetaViewModel> SelectAllFramed()
        {
            ObservableCollection<MetaViewModel> mvms=new ObservableCollection<MetaViewModel>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT * FROM TB_METADATA WHERE FRAMEPATH IS NOT NULL ORDER BY ID DESC;";
                IEnumerable<dynamic> dynamics=conn.Query(sql);
                foreach (var item in dynamics)
                {
                    MetaViewModel mvm = new MetaViewModel();
                    mvm.Id = (int)item.ID;
                    MetaModel mm = new MetaModel();
                    mm.VideoPath = item.VIDEOPATH;
                    mm.Address = item.ADDR;
                    mm.TaskCode = item.TASKCODE;
                    mm.StartTime = item.STARTTIME;
                    mm.FramePath = item.FRAMEPATH;
                    mvm.Meta = mm;
                    mvms.Add(mvm);
                }
                return mvms;
            }
        }

        public int UpdateInterval(string path, int i)
        {
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"UPDATE TB_METADATA SET INTERVAL=@I WHERE VIDEOPATH=@PATH";
                int result = conn.Execute(sql, new
                {
                    @I = i,
                    @PATH = path
                });
                return result;
            }
        }
    }
}
