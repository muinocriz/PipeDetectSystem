/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:MvvmLight4.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using MvvmLight4.Model;

namespace MvvmLight4.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        public static string FolderPath;
        public static string SavePath;
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }
            
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ModelManageViewModel>();
            SimpleIoc.Default.Register<ImportViewModel>(); 
            SimpleIoc.Default.Register<FrameViewModel>();
            SimpleIoc.Default.Register<TrainViewModel>();
            SimpleIoc.Default.Register<ExportViewModel>();
            SimpleIoc.Default.Register<DetectViewModel>();
            SimpleIoc.Default.Register<VideoViewModel>();
            SimpleIoc.Default.Register<BackTrackViewModel>();
            SimpleIoc.Default.Register<MarkViewModel>();
            SimpleIoc.Default.Register<MarkFileChooseViewModel>();
            SimpleIoc.Default.Register<SetViewModel>();
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
        public ModelManageViewModel ModelManage
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ModelManageViewModel>();
            }
        }

        public ImportViewModel Import
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ImportViewModel>();
            }
        }
        public FrameViewModel Frame
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FrameViewModel>();
            }
        }
        public TrainViewModel Train
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TrainViewModel>();
            }
        }
        public ExportViewModel Export
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ExportViewModel>();
            }
        }
        public DetectViewModel Detect
        {
            get
            {
                //return ServiceLocator.Current.GetInstance<DetectViewModel>();
                return new DetectViewModel();
            }
        }
        public VideoViewModel Video
        {
            get
            {
                return ServiceLocator.Current.GetInstance<VideoViewModel>();
            }
        }
        public BackTrackViewModel BackTrack
        {
            get
            {
                return ServiceLocator.Current.GetInstance<BackTrackViewModel>();
            }
        }
        public MarkViewModel MarkVM
        {
            get
            {
                //return ServiceLocator.Current.GetInstance<MarkViewModel>();
                //return new MarkViewModel(FolderPath, SavePath);
                return new MarkViewModel();
            }
        }
        public MarkFileChooseViewModel MarkFileChoose
        {
            get
            {
                //return ServiceLocator.Current.GetInstance<MarkFileChooseViewModel>();
                return new MarkFileChooseViewModel();
            }
        }

        public SetViewModel SetVM
        {
            get
            {
                //return ServiceLocator.Current.GetInstance<MarkFileChooseViewModel>();
                return new SetViewModel();
            }
        }
        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }
    }
}