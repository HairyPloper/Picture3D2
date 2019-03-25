﻿using System;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
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
        public CancellationTokenSource tokenSource = new CancellationTokenSource();


        public MainWindow(Window1 win, int n)
        {
            this.n = n;
            _windowOne = win;
            InitializeComponent();
            LoadImage();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            mainWindow = this;

            progressWorker.DoWork += progressWorker_DoWork;
            progressWorker.WorkerReportsProgress = true;
            progressWorker.RunWorkerCompleted += progressWorker_RunWorkerCompleted;
            progressWorker.ProgressChanged += ProgressWorker_ProgressChanged;
            progressWorker.WorkerSupportsCancellation = true;
            //progressWorker.RunWorkerAsync();
        }

        private void ProgressWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            ProgressBar.Value = e.ProgressPercentage;
        }

        public void ChangeProgressBar(int i)
        {
            ProgressBar.Value = i;
        }

        public static MainWindow GetMainWindow()
        {
            return mainWindow;
        }
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly BackgroundWorker progressWorker = new BackgroundWorker();

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

        private void progressWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker1 = sender as BackgroundWorker;
            worker1.ReportProgress(0);
            for (int i = 0; i <= 98; i++)
            {
                if (worker1.CancellationPending)
                {
                    worker1.ReportProgress(100);
                    Thread.Sleep(100);
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(100+i*30);
                worker1.ReportProgress(i);
            }
            //TO LONG
            while (true)
            {
                if (worker1.CancellationPending)
                {
                    worker1.ReportProgress(100);
                    Thread.Sleep(100);
                    e.Cancel = true;
                    break;
                }
            }
        }

        private void progressWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //MessageBox.Show("Process finished!");
            ProgressBar.Visibility = Visibility.Hidden;
            ProgressBar.Value = 0;
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
            progressWorker.RunWorkerAsync();
            ProgressBar.Visibility = Visibility.Visible;

            Task output = null;
            sample = true;
            try
            {
                output = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ExpGenMethod(AnaglyphParameters.VideoPath);
                    }
                    catch
                    {

                    }
                    progressWorker.CancelAsync();

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
                VideoToFrames.videoToFrames.ReadFromVideoHundred(path.LocalPath);
                //VideoToFrames.OnProcessDone += (sender, e) => VideoToFramesOnOnProcessDone(sender, e);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Window3 newWindow = new Window3(path.LocalPath.Split('.')[0] + "-converted" + ".mp4");
                    newWindow.Show();

                });
            }
            else
            {
                VideoToFrames.videoToFrames.ReadFromVideo(path.LocalPath);

                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Window3 newWindow = new Window3(path.LocalPath.Split('.')[0] + "-converted" + ".mp4");
                    newWindow.Show();

                });
            }
            notifyDone();
        }

        private void VideoToFramesOnOnProcessDone(object sender, EventArgs e)
        {
            var ev = (NotifyEventArgs)e;
            ProgressBar.Value = ev.Progress;
        }

        private void ApplyToVideo_Click(dynamic sender, RoutedEventArgs e)
        {
            progressWorker.RunWorkerAsync();
            ProgressBar.Visibility = Visibility.Visible;
            sample = false;
            try
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ExpGenMethod(AnaglyphParameters.VideoPath);
                    }
                    catch (Exception exception)
                    {

                    }
                    progressWorker.CancelAsync();
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
        }

        public void notifyDone()
        {
            progressWorker.CancelAsync();
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
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
    }
}
