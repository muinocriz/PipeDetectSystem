using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
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

        /// <summary>
        /// 分帧界面
        /// 检测视频是否已导入
        /// 已弃用
        /// </summary>
        /// <param name="path">视频位置</param>
        /// <returns></returns>
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

        /// <summary>
        /// 分帧文件选择界面
        /// 加载已导入未分帧的视频信息
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<MetaModel> SelectNotFrame()
        {
            ObservableCollection<MetaModel> models = new ObservableCollection<MetaModel>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT * FROM TB_METADATA 
                                WHERE FRAMEPATH IS NULL OR FRAMEPATH = '' 
                                ORDER BY ID DESC;";
                IEnumerable<dynamic> dynamics = conn.Query<MetaModel>(sql).ToList();
                foreach (var item in dynamics)
                {
                    models.Add(item);
                }
                return models;
            }
        }

        /// <summary>
        /// 导入界面
        /// 导入数据
        /// </summary>
        /// <param name="meta">元信息类</param>
        /// <returns>行数</returns>
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
                                                                        VIDEOPATH,
                                                                        HEADTIME,
                                                                        TAILTIME,
                                                                        GC) VALUES(
                                                                        @PipeCode,
                                                                        @PipeType,
                                                                        @TaskCode,
                                                                        @Addr,
                                                                        @Charge,
                                                                        @StartTime,
                                                                        @VideoPath,
                                                                        @HeadTime,
                                                                        @TailTime,
                                                                        @GC)",
                    new
                    {
                        meta.PipeCode,
                        meta.PipeType,
                        meta.TaskCode,
                        meta.Addr,
                        meta.Charge,
                        meta.StartTime,
                        meta.VideoPath,
                        meta.HeadTime,
                        meta.TailTime,
                        meta.GC
                    });
            }
            return insertedRows;
        }

        /// <summary>
        /// 标注视频选择窗口
        /// 根据选择的视频ID返回分帧之后文件所在位置
        /// </summary>
        /// <param name="id">视频ID</param>
        /// <returns>分帧之后文件所在位置</returns>
        public string QueryFramePathById(int id)
        {
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT DISTINCT FRAMEPATH FROM TB_METADATA WHERE ID = @ID;";
                string framePath = Convert.ToString(conn.Query(sql, new { ID = id }).FirstOrDefault().FRAMEPATH);
                return framePath;
            }
        }

        /// <summary>
        /// 标注界面
        /// 获取已分帧的视频列表
        /// </summary>
        /// <returns></returns>
        public List<ComplexInfoModel> QueryVideoFramed()
        {
            List<ComplexInfoModel> l = new List<ComplexInfoModel>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT ID,TASKCODE 
                                FROM TB_METADATA 
                                WHERE FRAMEPATH IS NOT NULL 
                                ORDER BY ID DESC;";
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

        /// <summary>
        /// 检测文件选择界面
        /// 获取所有分帧的视频列表
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<MetaViewModel> SelectAllFramed()
        {
            ObservableCollection<MetaViewModel> mvms = new ObservableCollection<MetaViewModel>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT * FROM TB_METADATA WHERE FRAMEPATH IS NOT NULL ORDER BY ID DESC;";
                IEnumerable<dynamic> dynamics = conn.Query(sql);
                foreach (var item in dynamics)
                {
                    MetaViewModel mvm = new MetaViewModel();
                    mvm.Id = (int)item.ID;
                    MetaModel mm = new MetaModel();
                    mm.VideoPath = item.VIDEOPATH;
                    mm.Addr = item.ADDR;
                    mm.TaskCode = item.TASKCODE;
                    mm.StartTime = item.STARTTIME;
                    mm.FramePath = item.FRAMEPATH;
                    mvm.Meta = mm;
                    mvms.Add(mvm);
                }
                return mvms;
            }
        }

        /// <summary>
        /// 输出界面
        /// 获取所有已检测过的视频信息
        /// </summary>
        /// <returns>包含视频id和项目信息的类</returns>
        public ObservableCollection<ExportMeta> SelectAllDetected()
        {
            ObservableCollection<ExportMeta> models = new ObservableCollection<ExportMeta>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT DISTINCT VIDEOID,TASKCODE 
                                FROM TB_METADATA,TB_ABNORMAL 
                                WHERE TB_METADATA.ID = TB_ABNORMAL.VIDEOID 
                                ORDER BY VIDEOID DESC 
                                LIMIT 500;";
                IEnumerable<dynamic> dynamics = conn.Query(sql);
                foreach (var item in dynamics)
                {
                    ExportMeta exportMeta = new ExportMeta
                    {
                        VideoId = Convert.ToInt32(item.VIDEOID),
                        TaskCode = item.TASKCODE
                    };
                    models.Add(exportMeta);
                }
                return models;
            }
        }

        /// <summary>
        /// 分帧页面
        /// 将分帧位置保存到数据库
        /// </summary>
        /// <param name="FramePath">分帧文件夹位置</param>
        /// <param name="VideoPath">源文件位置</param>
        /// <returns></returns>
        public int UpdateFramePathByVideoPath(string FramePath, string VideoPath)
        {
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"UPDATE TB_METADATA SET FRAMEPATH=@FramePath WHERE VIDEOPATH=@VideoPath";
                int result = conn.Execute(sql, new
                {
                    FramePath,
                    VideoPath
                });
                return result;
            }
        }

        /// <summary>
        /// 分帧界面
        /// 更新分帧间距
        /// 已弃用
        /// </summary>
        /// <param name="path">视频位置</param>
        /// <param name="i">每秒帧数</param>
        /// <returns></returns>
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
