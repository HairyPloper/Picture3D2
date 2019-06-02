using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Picture3D
{
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        string Path { get; set; }
        DispatcherTimer timer;
        public Window3(string Path)
        {
            this.Path = Path;
            InitializeComponent();
            IsPlaying(false);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += timer_Tick;
            MediaEL.Source = new Uri(Path);
            btnPlay.IsEnabled = true;
            InitialPlay();
        }
        private void InitialPlay()
        {
            IsPlaying(true);
            if (btnPlay.Content.ToString() == "Play")
            {
                MediaEL.Play();
                btnPlay.Content = "Pause";
            }
            else
            {
                MediaEL.Pause();
                btnPlay.Content = "Play";
            }
        }
        private void IsPlaying(bool bValue)
        {
            btnStop.IsEnabled = bValue;
            btnPlay.IsEnabled = bValue;
           
            seekBar.IsEnabled = bValue;
        }
        #region Play and Pause
        public void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            // VideoToFrames.ReadFromVideo();
            IsPlaying(true);
            if (btnPlay.Content.ToString() == "Play")
            {
                MediaEL.Play();
                btnPlay.Content = "Pause";
            }
            else
            {
                MediaEL.Pause();
                btnPlay.Content = "Play";
            }
        }
        #endregion

        #region Stop
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            MediaEL.Stop();
            btnPlay.Content = "Play";
            IsPlaying(false);
            btnPlay.IsEnabled = true;
        }
        #endregion

        #region Back and Forward
        private void btnMoveForward_Click(object sender, RoutedEventArgs e)
        {
            MediaEL.Position = MediaEL.Position + TimeSpan.FromSeconds(10);
        }

        private void btnMoveBackward_Click(object sender, RoutedEventArgs e)
        {
            MediaEL.Position = MediaEL.Position - TimeSpan.FromSeconds(10);
        }
        #endregion

        #region Open Media
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Video file (*.avi;*.mp4,*.wmv)|*.avi;*.mp4;*.wmv";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MediaEL.Source = new Uri(ofd.FileName);
                btnPlay.IsEnabled = true;
            }
        }

        #endregion
        #region Seek Bar
        private void MediaEL_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (MediaEL.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = MediaEL.NaturalDuration.TimeSpan;
                seekBar.Maximum = ts.TotalSeconds;
                seekBar.SmallChange = 1;
                seekBar.LargeChange = Math.Min(10, ts.Seconds / 10);
            }
            timer.Start();

        }

        bool isDragging = false;

        void timer_Tick(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                seekBar.Value = MediaEL.Position.TotalSeconds;
                currentposition = seekBar.Value;
            }
        }

        private void seekBar_DragStarted(object sender, DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void seekBar_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isDragging = false;
            MediaEL.Position = TimeSpan.FromSeconds(seekBar.Value);
        }
        #endregion

        #region FullScreen
        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();

        System.Timers.Timer timeClick = new System.Timers.Timer((int)GetDoubleClickTime())
        {
            AutoReset = false
        };

        bool fullScreen = false;
        double currentposition = 0;

        private void MediaEL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            btnPlay_Click(sender, e);
            if (!timeClick.Enabled)
            {

                timeClick.Enabled = true;
                return;
            }

            if (timeClick.Enabled)
            {
                if (!fullScreen)
                {
                    LayoutRoot.Children.Remove(MediaEL);
                    this.Background = new SolidColorBrush(Colors.Black);
                    this.Content = MediaEL;
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                    MediaEL.Position = TimeSpan.FromSeconds(currentposition);
                }
                else
                {
                    this.Content = LayoutRoot;
                    LayoutRoot.Children.Add(MediaEL);
                    this.Background = new SolidColorBrush(Colors.White);
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                    MediaEL.Position = TimeSpan.FromSeconds(currentposition);
                }
                fullScreen = !fullScreen;
            }
        }
        #endregion
        #region ChangeMediaVolume
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            MediaEL.Volume = (double)volumeSlider.Value;
        }
        void InitializePropertyValues()
        {
            MediaEL.Volume = (double)volumeSlider.Value;
        }
        #endregion

        private void VolumeSlider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                volumeSlider.Value += 0.05;
            else
                volumeSlider.Value -= 0.05;
        }
    }
}
