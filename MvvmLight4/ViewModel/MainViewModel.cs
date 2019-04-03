using System;
using System.Diagnostics;
using System.IO;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MvvmLight4.Model;
using MvvmLight4.Service;

namespace MvvmLight4.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            AssignCommands();
        }

        public RelayCommand LoadedCmd { get; private set; }
        private void AssignCommands()
        {
            LoadedCmd = new RelayCommand(() => ExecuteLoadedCmd());
        }

        private void ExecuteLoadedCmd()
        {
            string dbName = "data.sqlite";
            if (!File.Exists(dbName))
            {
                int result = InitService.GetService().InitDatabase(dbName);
                if (result < 0)
                {
                    Debug.WriteLine("创建出错");
                }
                else
                {
                    Debug.WriteLine("创建成功：{0}", result);
                }
            }
        }
    }
}