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

namespace Picture3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int n;
        private string baseURI = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private Window1 _windowOne;
        private bool sample = false;
        public static MainWindow mainWindow;
        private int _workerState;
        public int WorkerState
        {
            get { return _workerState; }
            set
            {
                _workerState = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("WorkerState"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;


        public MainWindow(Window1 win, int n)
        {
            this.n = n;
            _windowOne = win;
            InitializeComponent();
            LoadImage();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            CurrentAlgorythm = "";
            mainWindow = this;

        }
        public static MainWindow GetMainWindow()
        {
            return mainWindow;
        }
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private string CurrentAlgorythm { get; set; }

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


            AnaglyphParameters.ResetParameters();
            SetFilterValues();
            filterPanel.IsEnabled = true;
            ConvertedImage.Source = null;

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

        private void SaveBitmapImage(Bitmap image, out string outfilename)
        {
            outfilename = baseURI + @"\ScreenShots\Capture" + n + "-CONVERTED.jpg";
            try
            {
                image.Save(outfilename, ImageFormat.Jpeg);
                image.Dispose();

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

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

            //newImage.Dispose();
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
            sample = true;
            try
            {

                Task.Factory.StartNew(() =>
                    ExpGenMethod(AnaglyphParameters.VideoPath));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
        }

        public void ExpGenMethod(Uri path)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i <= 100; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        //100%
                        WorkerState = 100;
                        token.ThrowIfCancellationRequested();
                    }
                    Thread.Sleep(100);
                    WorkerState = i;
                }
            }, token);

            if (sample)
            {
                VideoToFrames.videoToFrames.ReadFromVideoHundred(path.LocalPath);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Window3 newWindow = new Window3(path.LocalPath.Split('.')[0] + "1.mp4");
                    newWindow.Show();

                });
            }
            else
            {
                VideoToFrames.videoToFrames.ReadFromVideo(path.LocalPath);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Window3 newWindow = new Window3(path.LocalPath.Split('.')[0] + "1.mp4");
                    newWindow.Show();

                });
            }

            tokenSource.Cancel();
            //Small delay to show 100% on progress bar
            Thread.Sleep(100);
        }
        private void ApplyToVideo_Click(dynamic sender, RoutedEventArgs e)
        {
            sample = false;

            try
            {

                Task.Factory.StartNew(() =>
                    ExpGenMethod(AnaglyphParameters.VideoPath));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }
}
