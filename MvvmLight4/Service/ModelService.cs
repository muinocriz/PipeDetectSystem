using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLight4.Common;
using MvvmLight4.Model;
using Dapper;
using MvvmLight4.ViewModel;

namespace MvvmLight4.Service
{
    class ModelService : IModelService
    {
        private static ModelService modelService = new ModelService();
        private ModelService() { }
        public static ModelService GetService()
        {
            return modelService;
        }
        /// <summary>
        /// 添加模型
        /// </summary>
        /// <param name="modelModel"></param>
        /// <returns></returns>
        public int AddModel(ModelModel modelModel)
        {
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"INSERT INTO TB_MODEL(MODELNAME,CREATETIME,LOCATION,UPDATETIME,SIMPLE,RATE,LTERATION) VALUES(@ModelName,@CreateTime,@Location,@UpdateTime,@Simple,@Rate,@Lteration)";
                return conn.Execute(sql, new
                {
                    modelModel.ModelName,
                    modelModel.CreateTime,
                    modelModel.Location,
                    modelModel.UpdateTime,
                    modelModel.Simple,
                    modelModel.Rate,
                    modelModel.Lteration
                });
            }
        }

        /// <summary>
        /// 删除指定模型
        /// </summary>
        /// <param name="modelViewModel"></param>
        /// <returns></returns>
        public int DeleteModel(ModelViewModel modelViewModel)
        {
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                var sql = @"DELETE FROM TB_MODEL WHERE ID=@Id";
                return conn.Execute(sql, new
                {
                    Id = modelViewModel.Id
                });
            }
        }

        /// <summary>
        /// 加载所有模型的详细信息
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<ModelViewModel> LoadData()
        {
            ObservableCollection<ModelViewModel> mvms = new ObservableCollection<ModelViewModel>();
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"SELECT * FROM TB_MODEL ORDER BY ID DESC;";
                IEnumerable<dynamic> dynamics = conn.Query(sql);
                foreach (var item in dynamics)
                {
                    ModelViewModel mvm = new ModelViewModel();
                    ModelModel mm = new ModelModel();
                    mm.CreateTime = item.CREATETIME;
                    mm.ModelName = item.MODELNAME;
                    mm.Location = item.LOCATION;
                    mvm.ModelModel = mm;
                    mvm.Id = (int)item.ID;
                    mvms.Add(mvm);
                }
            }
            return mvms;
        }

        /// <summary>
        /// 修改模型名字
        /// </summary>
        /// <param name="modelViewModel"></param>
        /// <returns></returns>
        public int UpdateModel(ModelViewModel modelViewModel)
        {
            using (IDbConnection conn = SqlHelper.GetConnection())
            {
                conn.Open();
                var sql = @"UPDATE TB_MODEL SET MODELNAME = @ModelName,UPDATETIME=@UpdateTime WHERE ID = @Id";
                return conn.Execute(sql, new
                {
                    modelViewModel.ModelModel.ModelName,
                    modelViewModel.ModelModel.UpdateTime,
                    modelViewModel.Id
                });
            }
        }
    }
}
