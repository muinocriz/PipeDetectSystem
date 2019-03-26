using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MvvmLight4.View
{
    /// <summary>
    /// VideoPlayWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPlayWindow
    {
        public VideoPlayWindow()
        {
            InitializeComponent();
        }

        DispatcherTimer timer = null;

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Play();
            //timer.Start();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            myMediaElement.Stop();
        }

        private void SetHead_Click(object sender, RoutedEventArgs e)
        {
            int SliderValue = (int)timelineSlider.Value;
            Console.WriteLine("SliderValue: " + SliderValue);
        }

        private void SetTail_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Element_MediaOpened(object sender, EventArgs e)
        {
            //设置滑动条最大值
            timelineSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
        }

        private void Timer_tick(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate
            {
                timelineSlider.Value = myMediaElement.Position.TotalSeconds;
            }));
        }

        private void Element_MediaEnded(object sender, EventArgs e)
        {
            myMediaElement.Stop();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += new EventHandler(Timer_tick);
        }

        private void TimelineSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            return;
        }

        private void TimelineSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            return;
        }

        private void TimelineSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            //timer.Stop();
        }

        private void TimelineSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            int SliderValue = (int)timelineSlider.Value;
            myMediaElement.Position = TimeSpan.FromSeconds(SliderValue);

           // timer.Start();
        }
    }
}
