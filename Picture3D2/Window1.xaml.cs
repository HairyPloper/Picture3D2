using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using Picture3D.AnaglyphApi;
using Color = System.Windows.Media.Color;
using System.Windows.Interop;
using Picture3D;
using System.Diagnostics;

namespace MediaSampleWPF
{
    public partial class Window1 : Window
    {
        DispatcherTimer timer;
        private int n = 0;

        #region Constructor
        public Window1()
        {
            InitializeComponent();
            IsPlaying(false);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += new EventHandler(timer_Tick);
        }
        #endregion

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

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

        #region IsPlaying(bool)
        private void IsPlaying(bool bValue)
        {
            btnStop.IsEnabled = bValue;
            btnMoveBackward.IsEnabled = bValue;
            btnMoveForward.IsEnabled = bValue;
            btnPlay.IsEnabled = bValue;
            btnScreenShot.IsEnabled = bValue;
            seekBar.IsEnabled = bValue;
        }
        #endregion

        #region Play and Pause
        private void btnPlay_Click(object sender, RoutedEventArgs e)
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

        #region Capture Screenshot

        private void btnScreenShot_Click(object sender, RoutedEventArgs e)
        {
            
            MediaEL.Pause();
            btnPlay.Content = "Play";
            string sMessageBoxText = "Do you want to use this screenshot?";
            string sCaption = "Confirm screenshot";

            MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
            MessageBoxImage icnMessageBox = MessageBoxImage.Question;

            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    byte[] screenshot = MediaEL.GetScreenShot(1, 90);
                    string baseURI = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string pathString = System.IO.Path.Combine(baseURI, "ScreenShots");
                    System.IO.Directory.CreateDirectory(pathString);
                    using (FileStream fileStream = new FileStream(baseURI + @"\ScreenShots\Capture" + n + ".jpg", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        BinaryWriter binaryWriter = new BinaryWriter(fileStream);
                        binaryWriter.Write(screenshot);
                        fileStream.Close();
                    }
                    

                    this.Visibility = Visibility.Hidden;
                    this.IsEnabled = false;
                    MainWindow two = new MainWindow(this,n);
                    two.Show();
                    n++;
                    break;

                case MessageBoxResult.No:
                    MediaEL.Play();
                    btnPlay.Content = "Pause";
                    break;
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

    }

    #region Extension Methods

    public static class ScreenShot
    {

        public static byte[] GetScreenShot(this UIElement source, double scale, int quality)
        {

            double actualHeight = source.RenderSize.Height;
            double actualWidth = source.RenderSize.Width;
            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)actualWidth,
                (int)actualHeight, 96, 96, PixelFormats.Pbgra32);
            VisualBrush sourceBrush = new VisualBrush(source);
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            using (drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new System.Windows.Point(0, 0),
                    new System.Windows.Point(actualWidth, actualHeight)));
            }
            renderTarget.Render(drawingVisual);
            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.QualityLevel = quality;
            jpgEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

            Byte[] imageArray;

            using (MemoryStream outputStream = new MemoryStream())
            {
                jpgEncoder.Save(outputStream);
                imageArray = outputStream.ToArray();
                
            }
            return imageArray;
        }
    }


    #endregion


}
