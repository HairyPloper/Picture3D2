
using System;
using System.Drawing;
using Accord.Audio;
using Accord.Video.FFMPEG;
using Accord.Audio.Formats;
using Accord.DirectSound;
using AnaglyphGenerator.Models;


namespace Picture3D.AnaglyphApi
{
    class VideoToFrames
    {
        public static VideoToFrames videoToFrames { get; } = new VideoToFrames();
        VideoFileReader reader;
        VideoFileWriter writer;
        private WaveFileAudioSource audioSoruce;
        string pathToWrite = "";
        Uri pathToFile;

        private VideoToFrames()
        {

            reader = new VideoFileReader();
            writer = new VideoFileWriter();
        }

        public void ReadFromVideo(string path)
        {

            pathToFile = new Uri(path);
            reader.Open(pathToFile.LocalPath);
            pathToWrite = pathToFile.LocalPath.Split('.')[0] + "1.mp4";
            audioSoruce = new WaveFileAudioSource(pathToFile.LocalPath);
            SetWriter(reader, writer);
            for (int i = 0; i < reader.FrameCount; i++)
            {
                try
                {
                    using (Bitmap videoFrame = reader.ReadVideoFrame(i))
                    {
                        using (Bitmap videoFrameChanged = new Fitler().Calc(videoFrame))
                        {
                            //videoFrameChanged.Save(Application.StartupPath + "\\img.bmp");
                            if (videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ||
                                videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                                writer.WriteVideoFrame(videoFrameChanged, (uint) i);
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
                        using (Bitmap videoFrameChanged = new Fitler().Calc(videoFrame))
                        {
                            //videoFrameChanged.Save(Application.StartupPath + "\\img.bmp");
                            if (videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ||
                                videoFrameChanged.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppRgb)
                                writer.WriteVideoFrame(videoFrameChanged, (uint) i);
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

        public void SetWriter(VideoFileReader reader, VideoFileWriter writer)
        {
            switch (reader.CodecName.ToUpper())
            {
                case "MPEG4":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MPEG4,
                        reader.BitRate);
                    break;
                case "WMV1":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.WMV1,
                        reader.BitRate);
                    break;
                case "WMV2":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.WMV2,
                        reader.BitRate);
                    break;
                case "MSMPEG4v2":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MSMPEG4v2,
                        reader.BitRate);
                    break;
                case "MSMPEG4v3":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MSMPEG4v3,
                        reader.BitRate);
                    break;
                case "H263P":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.H263P,
                        reader.BitRate);
                    break;
                case "FLV1":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.FLV1,
                        reader.BitRate);
                    break;
                case "MPEG2":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MPEG2,
                        reader.BitRate);
                    break;
                case "Raw":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.Raw,
                        reader.BitRate);
                    break;
                case "FFV1":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.FFV1,
                        reader.BitRate);
                    break;
                case "FFVHUFF":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.FFVHUFF,
                        reader.BitRate);
                    break;
                case "H264":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.H264,
                        reader.BitRate);
                    break;
                case "H265":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.H265,
                        reader.BitRate);
                    break;
                case "Theora":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.Theora,
                        reader.BitRate);
                    break;
                case "VP8":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.VP8,
                        reader.BitRate);
                    break;
                case "VP9":
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.VP9,
                        reader.BitRate);
                    break;
                default:
                    writer.Open(pathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.Default,
                        reader.BitRate);
                    break;
            }
        }

    }
}
