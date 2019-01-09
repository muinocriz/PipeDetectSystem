using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using MvvmLight4.ViewModel;

namespace MvvmLight4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }

        private Assembly _assembly = Assembly.GetExecutingAssembly();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string winName = ((Button)e.OriginalSource).Tag.ToString();
            Window win = ((Window)_assembly.CreateInstance(string.Format("MvvmLight4.View.{0}", winName)));
            win.Owner = this;
            win.Show();
        }
    }
}