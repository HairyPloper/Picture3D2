using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AnaglyphGenerator.Models;
using Microsoft.Win32;
using Picture3D.AnaglyphApi;
using Color = System.Windows.Media.Color;

namespace Picture3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;

            CurrentAlgorythm = "";

        }
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private string CurrentAlgorythm { get; set; }


        private void LoadImageMenu_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = "c:\\";
            dlg.Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == true)
            {
                string selectedFileName = dlg.FileName;
                MainImageTextBox.Text = selectedFileName;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedFileName);
                bitmap.EndInit();
                MainImage.Source = bitmap;

                filterPanel.IsEnabled = true;
                //Reset filters and static values
                AnaglyphParameters.ResetParameters();
                SetFilterValues();
                ConvertedImage.Source = null;
            }
        }

        private void SaveImageMenu_Click(object sender, RoutedEventArgs e)
        {
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
        private void ColorSlider_ValueChangedX(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnaglyphParameters.Xaxis = slXaxis.Value;
        }
        private void ColorSlider_ValueChangedY(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AnaglyphParameters.Yaxis = slYaxis.Value;
        }


        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ColorAnaglyphMenu_Click(dynamic sender, RoutedEventArgs e)
        {
            string selectedAlgorithm = "";
            if (sender.Name == "RegenerateButton")
            {
                selectedAlgorithm = CurrentAlgorythm;
            }
            else
            {
             selectedAlgorithm = sender.Header;

            }
            string imgLocation = "";
            Bitmap imagebmp;
            try
            {
                if (MainImage.Source == null)
                    throw new Exception("Load image first.");

                if (this.CurrentAlgorythm != selectedAlgorithm)
                {
                    AnaglyphParameters.ResetParameters();
                    SetFilterValues();
                    CurrentAlgorythm = selectedAlgorithm;
                }

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
                MessageBox.Show(exception.Message);
            }
        }
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapImage retval;

            try
            {
                retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }

        private void SaveBitmapImage(Bitmap image, string selectedAlgorithm, out string outfilename)
        {
            Random rnd = new Random();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            outfilename = selectedAlgorithm.Replace(" ", "_").ToLower() + "-" + rnd.Next(0, 2315412) + ".jpeg";

            image.Save(outfilename);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundHelperRequest argmunets = (BackgroundHelperRequest)e.Argument;
            string imgLocation;
            //AlgorythmParameters parameters = null;

            //Call Algorithm
            Bitmap newImage = new AnaglyphAlgorithmInvoker(argmunets.selectedAlgorythm).Apply(argmunets.image);

            //Save bmp to root of app
            SaveBitmapImage(newImage, argmunets.selectedAlgorythm, out imgLocation);
            var path = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            string fullpath = path + @"\" + imgLocation;

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
            SetFilterValues();
        }

        private void SetFilterValues()
        {
            slColorB.Value = AnaglyphParameters.BlueVolume;
            slColorR.Value = AnaglyphParameters.RedVolume;
            slColorG.Value = AnaglyphParameters.GreenVolume;
            slXaxis.Value = AnaglyphParameters.Xaxis;
            slYaxis.Value = AnaglyphParameters.Yaxis;
        }

        private void ClearImages_Click(object sender, RoutedEventArgs e)
        {
            MainImage.Source = null;
        }
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

}
