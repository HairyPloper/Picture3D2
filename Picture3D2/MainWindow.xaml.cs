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
        }
        private void ColorSlider_ValueChangedBlue(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnaglyphParameters.BlueVolume = slColorB.Value;
        }
        private void ColorSlider_ValueChangedGreen(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnaglyphParameters.GreenVolume = slColorG.Value;
        }

        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //public static extern bool DeleteObject(IntPtr hObject);

        //private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        //{
        //    IntPtr hBitmap = bitmap.GetHbitmap();
        //    BitmapImage retval;

        //    try
        //    {
        //        retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
        //            hBitmap,
        //            IntPtr.Zero,
        //            Int32Rect.Empty,
        //            BitmapSizeOptions.FromEmptyOptions());
        //    }
        //    finally
        //    {
        //        DeleteObject(hBitmap);
        //    }

        //    return retval;
        //}

        private void ColorAnaglyphMenu_Click(dynamic sender, RoutedEventArgs e)
        {
            string selectedAlgorithm = "";
            if (sender.Name == "RegenerateButton")
            {
                selectedAlgorithm = "Color Anaglyph";
            }
            else
            {
                selectedAlgorithm = sender.Header;

            }
            ConvertedImage.Source = null;
            string imgLocation = "";
            Bitmap imagebmp;
            try
            {
                if (MainImage.Source == null)
                    throw new Exception("Load image first.");

                //if (this.CurrentAlgorythm != selectedAlgorithm)
                //{
                //    AnaglyphParameters.ResetParameters();
                //    SetFilterValues();
                //    CurrentAlgorythm = selectedAlgorithm;
                //}

                imagebmp = new Bitmap((string)MainImageTextBox.Text); // location for image

                BackgroundHelperRequest arguments = new BackgroundHelperRequest()
                {
                    image = imagebmp,
                    selectedAlgorythm = selectedAlgorithm,
                };

                worker.RunWorkerAsync(argument: arguments);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                System.Windows.Forms.MessageBox.Show(exception.Message);
            }
           
        }

        private void SaveBitmapImage(Bitmap image, string selectedAlgorithm, out string outfilename)
        {         
            outfilename = baseURI + @"\ScreenShots\Capture" + n + "-CONVERTED.jpg";
            try
            {
                image.Save(outfilename,ImageFormat.Jpeg);
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
            string imgLocation;
            //AlgorythmParameters parameters = null;

            //Call Algorithm
            Bitmap newImage = new Bitmap(1,1);//new AnaglyphAlgorithmInvoker(argmunets.selectedAlgorythm).Apply(argmunets.image);

            //Save bmp to root of app
            SaveBitmapImage(newImage, argmunets.selectedAlgorythm, out imgLocation);
            var path = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            string fullpath =imgLocation;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullpath);
            bitmap.EndInit();
            bitmap.Freeze();
            e.Result = new BackgroundHelperResponse
            {
                image = bitmap,
                location = fullpath,
            };
            
            newImage.Dispose();
        }
        private void worker_RunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs e)
        {
            var response = (BackgroundHelperResponse)e.Result;
            ConvertedImage.Source = response.image;
            ConvertedImageTextBox.Text = response.location;

            //Update gui with new values for filters
            VideoToFrames.videoToFrames.ReadFromVideo();
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
            public Bitmap image;
            public string selectedAlgorythm;

        }
        public class BackgroundHelperResponse
        {
            public BitmapImage image;
            public string location;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(baseURI + @"/ScreenShots");
        }

    }
}
