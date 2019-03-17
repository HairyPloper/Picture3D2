
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Video.FFMPEG;
using AnaglyphGenerator.Models;
using System.Threading;
using System.Threading.Tasks;


namespace Picture3D.AnaglyphApi
{
    class VideoToFrames
    {
        public static VideoToFrames videoToFrames { get; } = new VideoToFrames();
        VideoFileReader reader;
        VideoFileWriter writer;
        string alghorithmType = "Color Anaglyph";
        string pathToWrite = "";
        Uri pathToFile;


        object lockVideoFrame = 0;
        object lockVideoFrameChanged = 0;
        object lockWriter = 0;
        object lockReader = 0;
        private VideoToFrames()
        {
            reader = new VideoFileReader();
            writer = new VideoFileWriter();
        }

        //public void ReadFromVideo()
        //{

        //    System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
        //    ofd.Filter = "Video file (*.avi;*.mp4,*.wmv)|*.avi;*.mp4;*.wmv";
        //    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        pathToFile = new Uri(ofd.FileName);
        //        reader.Open(pathToFile.LocalPath);
        //        pathToWrite = pathToFile.LocalPath.Split('.')[0] + "1.mp4";
        //        writer.Open(pathToWrite, reader.Width, reader.Height);
        //    }


        //    // open video file
        //    // reader.Open("small.mp4");

        //    // read 100 video frames out of it

        //    Parallel.For(0, 100, (i, state) =>
        //      {
        //                  Bitmap videoFrame = new Bitmap(720, 1280);
        //                  Bitmap videoFrameChanged = new Bitmap(720, 1280);
        //          //for (int i = 0; i < 100; i++)
        //          //{


        //        Monitor.Enter(lockVideoFrame);
        //          try
        //          {
        //              videoFrame = reader.ReadVideoFrame(i);
        //          }
        //          catch
        //          {

        //          }
        //          Monitor.Exit(lockVideoFrame);

        //          Monitor.Enter(lockVideoFrameChanged);
        //          try
        //          {
        //              videoFrameChanged = new AnaglyphAlgorithmInvoker("Color Anaglyph").Apply(videoFrame);
        //          }
        //          catch
        //          {

        //          }

        //          Monitor.Exit(lockVideoFrameChanged);


        //          Monitor.Enter(lockWriter);
        //          try
        //          {
        //              if (videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb || videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
        //                  writer.WriteVideoFrame(videoFrameChanged, (uint)i);
        //              videoFrameChanged.Dispose();
        //          }
        //          catch
        //          {

        //          }
        //        //videoFrameChanged.Save(Application.StartupPath + "\\img.bmp");



        //          videoFrame.Dispose();
        //          videoFrameChanged.Dispose();



        //      }
        //    );

        //    writer.Close();
        //    reader.Close();
        //}
        public void ReadFromVideo(string path)
        {

            pathToFile = new Uri(path);
            reader.Open(pathToFile.LocalPath);
            pathToWrite = pathToFile.LocalPath.Split('.')[0] + "1.mp4";

            SetWriter(reader, writer);
            for (int i = 0; i < reader.FrameCount; i++)
            {
                try
                {
                    using (Bitmap videoFrame = reader.ReadVideoFrame(i))
                    {
                        using (Bitmap videoFrameChanged = new AnaglyphAlgorithmInvoker(alghorithmType).Apply(videoFrame))
                        {
                            //videoFrameChanged.Save(Application.StartupPath + "\\img.bmp");
                            if (videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb || videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                                writer.WriteVideoFrame(videoFrameChanged, (uint)i);
                            videoFrameChanged.Dispose();
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }


            }
            writer.Close();
            reader.Close();
        }
        public void ReadFromVideoHundred(string path)
        {

            pathToFile = new Uri(path);
            reader.Open(pathToFile.LocalPath);
            pathToWrite = pathToFile.LocalPath.Split('.')[0] + "1.mp4";

            SetWriter(reader, writer);
            for (int i = 0; i < 100; i++)
            {
               
                try
                {
                    using (Bitmap videoFrame = reader.ReadVideoFrame(i))
                    {
                        using (Bitmap videoFrameChanged = new AnaglyphAlgorithmInvoker(alghorithmType).Apply(videoFrame))
                        {
                            //videoFrameChanged.Save(Application.StartupPath + "\\img.bmp");
                            if (videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb || videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                                writer.WriteVideoFrame(videoFrameChanged, (uint)i);
                            videoFrameChanged.Dispose();
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }


            }
            writer.Close();
            reader.Close();
        }
        public void SetWriter (VideoFileReader reader, VideoFileWriter writer)
        {
            switch (reader.CodecName.ToUpper())
            {
                case "MPEG4":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MPEG4, reader.BitRate);
                    break;
                case "WMV1":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.WMV1, reader.BitRate);
                    break;
                case "WMV2":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.WMV2, reader.BitRate);
                    break;
                case "MSMPEG4v2":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MSMPEG4v2, reader.BitRate);
                    break;
                case "MSMPEG4v3":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MSMPEG4v3, reader.BitRate);
                    break;
                case "H263P":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.H263P, reader.BitRate);
                    break;
                case "FLV1":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.FLV1, reader.BitRate);
                    break;
                case "MPEG2":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MPEG2, reader.BitRate);
                    break;
                case "Raw":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.Raw, reader.BitRate);
                    break;
                case "FFV1":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.FFV1, reader.BitRate);
                    break;
                case "FFVHUFF":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.FFVHUFF, reader.BitRate);
                    break;
                case "H264":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.H264, reader.BitRate);
                    break;
                case "H265":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.H265, reader.BitRate);
                    break;
                case "Theora":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.Theora, reader.BitRate);
                    break;
                case "VP8":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.VP8, reader.BitRate);
                    break;
                case "VP9":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.VP9, reader.BitRate);
                    break;
                default:
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.Default, reader.BitRate);
                    break;
            }
        }

    }
}
