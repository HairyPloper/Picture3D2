using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using AnaglyphGenerator.Models;
using Picture3D.AnaglyphApi;
using MediaSampleWPF;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Picture3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private int n;
        private string baseURI = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private Window1 _windowOne;
        private bool sample = false;
        private bool includeAudio = false;
        private double curentTime = 0;
        public static MainWindow mainWindow;
        private string numOfFramesValue = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow(Window1 win, int n, double curentTime)
        {
            this.n = n;
            this.curentTime = curentTime;
            _windowOne = win;
            InitializeComponent();
            LoadImage();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            mainWindow = this;
        }

        private readonly BackgroundWorker worker = new BackgroundWorker();

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            _windowOne.Visibility = Visibility.Visible;
            _windowOne.IsEnabled = true;
            this.IsEnabled = false;
        }

        private void LoadImage()
        {
            string fullpath = baseURI + @"\ScreenShots\Capture" + n + ".jpg";
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullpath);
            bitmap.EndInit();
            MainImageTextBox.Text = fullpath;
            MainImage.Source = bitmap;
            ConvertedImage.Source = bitmap;


            AnaglyphParameters.ResetParameters();
            SetFilterValues();
            filterPanel.IsEnabled = true;

        }


        private void ColorSlider_ValueChangedRed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnaglyphParameters.RedVolume = slColorR.Value;
            RegenerateButton_Click(this, e);
        }
        private void ColorSlider_ValueChangedBlue(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnaglyphParameters.BlueVolume = slColorB.Value;
            RegenerateButton_Click(this, e);
        }
        private void ColorSlider_ValueChangedGreen(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnaglyphParameters.GreenVolume = slColorG.Value;
            RegenerateButton_Click(this, e);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundHelperRequest argmunets = (BackgroundHelperRequest)e.Argument;


            //Call Algorithm
            Bitmap newImage = new Fitler().Calc(argmunets.Image);

            e.Result = new BackgroundHelperResponse
            {
                Image = newImage,
                Location = "",
            };

        }
        private void worker_RunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs e)
        {
            var response = (BackgroundHelperResponse)e.Result;

            ConvertedImage.Source = ToBitmapImage(response.Image);
            SetFilterValues();
        }

        private void SetFilterValues()
        {
            slColorR.Value = AnaglyphParameters.RedVolume;
            slColorG.Value = AnaglyphParameters.GreenVolume;
            slColorB.Value = AnaglyphParameters.BlueVolume;
        }
        public class BackgroundHelperRequest
        {
            public Bitmap Image;
            public string SelectedAlgorythm;

        }
        public class BackgroundHelperResponse
        {
            public Bitmap Image;
            public string Location;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(baseURI + @"/ScreenShots");
        }
        private void SampleVideo_Click(dynamic sender, RoutedEventArgs e)
        {
            string PathToSave = SaveTo();
            if (String.IsNullOrEmpty(PathToSave))
            {
                return;
            }
            ProgressBar.Visibility = Visibility.Visible;
            numOfFramesValue = numOfFrames.Text;
            Task output = null;
            sample = true;
            try
            {
                output = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ExpGenMethod(new Uri(PathToSave));
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
        }

        public void ExpGenMethod(Uri path)
        {
            if (sample)
            {
                VideoToFrames.GetInstance().OnFrameDone += this.FrameDone;
                VideoToFrames.GetInstance().OnProcessDone += this.VideoToFramesOnProcessDone;
                VideoToFrames.GetInstance().ReadFromVideoSample(path.LocalPath, curentTime, numOfFramesValue, includeAudio);

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Window3 newWindow = new Window3(AnaglyphParameters.PathToWrite);
                        newWindow.Show();
                });
            }
            else
            {
                VideoToFrames.GetInstance().OnFrameDone += this.FrameDone;
                VideoToFrames.GetInstance().OnProcessDone += this.VideoToFramesOnProcessDone;
                VideoToFrames.GetInstance().ReadFromVideo(path.LocalPath);

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if(!File.Exists(AnaglyphParameters.PathToWrite) && AnaglyphParameters.HasSound)
                    {
                    Window3 newWindow = new Window3(path.LocalPath);
                        newWindow.Show();
                    }
                    else
                    {
                        Window3 newWindow = new Window3(AnaglyphParameters.PathToWrite);
                        newWindow.Show();
                    }


                });
            }
        }

        private void VideoToFramesOnProcessDone(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = 0;
                ProgressBar.Visibility = Visibility.Hidden;
            });
        }

        private void FrameDone(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                var ev = (NotifyEventArgs)e;
                double prog = ev.Progress / (double)ev.Total;

                ProgressBar.Value = prog * 100;
            });
        }

        private void ApplyToVideo_Click(dynamic sender, RoutedEventArgs e)
        {
            string PathToSave = SaveTo();
            if (String.IsNullOrEmpty(PathToSave))
            {
                return;
            }

            ProgressBar.Visibility = Visibility.Visible;
            sample = false;
            try
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ExpGenMethod(new Uri(PathToSave));
                    }
                    catch (Exception exception)
                    {

                    }
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
        }

        private void RegenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainImage.Source == null)
                throw new Exception("Load image first.");

            var imagebmp = new Bitmap((string)MainImageTextBox.Text); // location for image

            BackgroundHelperRequest arguments = new BackgroundHelperRequest()
            {
                Image = imagebmp,
                SelectedAlgorythm = "",
            };

            if (!worker.IsBusy)
                worker.RunWorkerAsync(arguments);
        }
        public BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        protected override void OnClosed(EventArgs e)
        {

            base.OnClosed(e);
            //Application.Current.Shutdown();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            AnaglyphParameters.RedVolume = 0;
            AnaglyphParameters.BlueVolume = 0;
            AnaglyphParameters.GreenVolume = 0;
            SetFilterValues();
        }
        private string SaveTo()
        {
            using (var fbd = new System.Windows.Forms.SaveFileDialog())
            {
                fbd.Filter = "Video file (*.mp4)|*.mp4;";
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                {
                    return fbd.FileName;
                }

            }
            return "";
        }
        private void SlColorR_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                slColorR.Value += 2.5;
            else
                slColorR.Value -= 2.5;
        }

        private void SlColorG_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                slColorG.Value += 2.5;
            else
                slColorG.Value -= 2.5;
        }

        private void SlColorB_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                slColorB.Value += 2.5;
            else
                slColorB.Value -= 2.5;
        }
        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            var check = (System.Windows.Controls.CheckBox)e.Source;
            if (check.IsChecked.Value)
            {
                BorderMainImage.Visibility = Visibility.Hidden;
                BorderConvertedImage.SetValue(Grid.ColumnProperty, 0);
                BorderConvertedImage.SetValue(Grid.ColumnSpanProperty, 2);
            }
            else
            {
                BorderMainImage.Visibility = Visibility.Visible;
                BorderConvertedImage.SetValue(Grid.ColumnProperty, 1);
                BorderConvertedImage.SetValue(Grid.ColumnSpanProperty, 1);
            }
        }

        //private void AudioCheckBoxChanged(object sender, RoutedEventArgs e)
        //{
        //    var check = (System.Windows.Controls.CheckBox)e.Source;
        //    if (check.IsChecked.Value)
        //    {
        //        includeAudio = true;
        //    }

        //}
    }
}
