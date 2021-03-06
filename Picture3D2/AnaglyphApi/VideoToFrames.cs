﻿
using System;
using System.Drawing;
using Accord.Video.FFMPEG;
using AForge.Video.DirectShow;
using Accord.DirectSound;
using AnaglyphGenerator.Models;
using System.IO;
using System.Diagnostics;
using System.Threading;


namespace Picture3D.AnaglyphApi
{
    class VideoToFrames
    {
        private static VideoToFrames videoToFrames { get; set; }
        VideoFileReader reader;
        VideoFileWriter writer;
        FilterInfoCollection CaptureDdevice;
        private WaveFileAudioSource audioSoruce;

        public delegate void OnFrameDoneHandler(object sender, EventArgs e);

        public event OnFrameDoneHandler OnFrameDone;
        public delegate void OnProcessDoneHandler(object sender, EventArgs e);

        public event OnProcessDoneHandler OnProcessDone;


        Uri pathToFile;


        private VideoToFrames()
        {

            reader = new VideoFileReader();
            writer = new VideoFileWriter();


        }
        public static VideoToFrames GetInstance()
        {
            if (videoToFrames == null)
                videoToFrames = new VideoToFrames();
            return videoToFrames;

        }
        public void ReadFromVideo(string path)
        {
            pathToFile = new Uri(AnaglyphParameters.VideoPath.LocalPath);
            AnaglyphParameters.PathToRead = pathToFile.LocalPath;
            AnaglyphParameters.PathToWrite = path.Split('.')[0] + "1.mp4";
            reader.Open(pathToFile.LocalPath);

            SetWriter(reader, writer);


            for (int i = 0; i < reader.FrameCount; i++)
            {
                OnOnFrameDone(i, (int)reader.FrameCount);
                try
                {

                    using (Bitmap videoFrame = reader.ReadVideoFrame(i))
                    {
                        using (Bitmap videoFrameChanged = new Fitler().Calc(videoFrame))
                        {
                            writer.WriteVideoFrame(videoFrameChanged, (uint)i);


                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }


            }
            reader.Close();
            writer.Close();

            VideoToFrames.AddAudioToVideo(path);
            OnOnProcessDone();
        }

        public void ReadFromVideoSample(string path, double curentTime, String numberOfFrames, bool includeAudio)
        {
            int number = 0;
            if (numberOfFrames != "")
                number = Convert.ToInt32(numberOfFrames);
            pathToFile = new Uri(AnaglyphParameters.VideoPath.LocalPath);
            reader.Open(pathToFile.LocalPath);
            int startingFrame = Convert.ToInt32(curentTime * reader.FrameRate.Value);
            int endingFrame = Convert.ToInt32(startingFrame + number);
            AnaglyphParameters.PathToRead = pathToFile.LocalPath;
            AnaglyphParameters.PathToWrite = path.Split('.')[0] + "1.mp4";// pathToFile.LocalPath.Split('.')[0] + "1.mp4";
            reader.Open(pathToFile.LocalPath);
            SetWriter(reader, writer);
            int j = 0;
            for (int i = startingFrame; i < endingFrame; i++)
            {
                OnOnFrameDone(j, number);
                try
                {
                    using (Bitmap videoFrame = reader.ReadVideoFrame(i))
                    {
                        using (Bitmap videoFrameChanged = new Fitler().Calc(videoFrame))
                        {

                            writer.WriteVideoFrame(videoFrameChanged, (uint)i);

                            videoFrameChanged.Dispose();
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }

                j++;
            }
            reader.Close();
            writer.Close();
            //string metadata = GetMetadataFromVideo(AnaglyphParameters.PathToRead);

            OnOnProcessDone();
        }


        public void SetWriter(VideoFileReader reader, VideoFileWriter writer)
        {
            switch (reader.CodecName.ToUpper())
            {
                case "MPEG4":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MPEG4,
                        reader.BitRate);
                    break;
                case "WMV1":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.WMV1,
                        reader.BitRate);
                    break;
                case "WMV2":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.WMV2,
                        reader.BitRate);
                    break;
                case "MSMPEG4v2":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MSMPEG4v2,
                        reader.BitRate);
                    break;
                case "MSMPEG4v3":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MSMPEG4v3,
                        reader.BitRate);
                    break;
                case "H263P":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.H263P,
                        reader.BitRate);
                    break;
                case "FLV1":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.FLV1,
                        reader.BitRate);
                    break;
                case "MPEG2":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.MPEG2,
                        reader.BitRate);
                    break;
                case "Raw":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.Raw,
                        reader.BitRate);
                    break;
                case "FFV1":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.FFV1,
                        reader.BitRate);
                    break;
                case "FFVHUFF":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.FFVHUFF,
                        reader.BitRate);
                    break;
                case "H264":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.H264,
                        reader.BitRate);
                    break;
                case "H265":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.H265,
                        reader.BitRate);
                    break;
                case "Theora":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.Theora,
                        reader.BitRate);
                    break;
                case "VP8":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.VP8,
                        reader.BitRate);
                    break;
                case "VP9":
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.VP9,
                        reader.BitRate);
                    break;
                default:
                    writer.Open(AnaglyphParameters.PathToWrite, reader.Width, reader.Height, reader.FrameRate, VideoCodec.Default,
                        reader.BitRate);
                    break;
            }
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public static void TakeAudioFromVideo(string pathToRead, string pathToWrite)
        {
            var inputFile = pathToRead;
            var outputFile = pathToRead.Split('.')[0] + ".aac";
            AnaglyphParameters.AudioFile = outputFile;
           if( File.Exists(AnaglyphParameters.AudioFile))
                File.Delete(AnaglyphParameters.AudioFile);
            var mp3out = "";
            var ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardInput = true;
            ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.CreateNoWindow = true;
            ffmpegProcess.StartInfo.FileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "ffmpeg\\ffmpeg.exe");//"C:\\Users\\Filip\\Source\\Repos\\Picture3D2\\Picture3D2\\ffmpeg-20190323-5fceac1-win32-shared\\bin\\ffmpeg.exe";//@"\ffmpeg-20190323-5fceac1-win32-shared\bin\ffmpeg.exe"; 
                                                                                                                                                     // ffmpegProcess.StartInfo.Arguments = " -i " + inputFile + " -vn -f mp3 -ab 320k output " + outputFile;
            ffmpegProcess.StartInfo.Arguments = "-i " + inputFile + " -vn -acodec copy " + outputFile; //ffmpeg -i input-video.avi -vn -acodec copy output-audio.aac
            ffmpegProcess.Start();
            ffmpegProcess.StandardOutput.ReadToEnd();
            mp3out = ffmpegProcess.StandardError.ReadToEnd();
            ffmpegProcess.WaitForExit();
            if (!ffmpegProcess.HasExited)
            {
                ffmpegProcess.Kill();
            }
            Console.WriteLine(mp3out);
            ffmpegProcess.Close();


        }

        public static void AddAudioToVideo(string path)
        {
            Random rnd = new Random();
            int num = rnd.Next(2, 100);
            TakeAudioFromVideo(AnaglyphParameters.PathToRead,path);
            var inputFile = AnaglyphParameters.PathToWrite;
            var outputFile = path;
            var audioFile = AnaglyphParameters.AudioFile;
            try
            {
                
                if(File.Exists(audioFile))
                   AnaglyphParameters.HasSound = true;
            }
            catch(Exception e)
            {
                AnaglyphParameters.HasSound = false;
            }
     
                                   // AnaglyphParameters.PathToRead.Split('.')[0] + ".aac";
            var mp3out = "";
            var ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardInput = true;
            ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.CreateNoWindow = true;
            ffmpegProcess.StartInfo.FileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "ffmpeg\\ffmpeg.exe");//"C:\\Users\\Filip\\Source\\Repos\\Picture3D2\\Picture3D2\\ffmpeg-20190323-5fceac1-win32-shared\\bin\\ffmpeg.exe";//@"\ffmpeg-20190323-5fceac1-win32-shared\bin\ffmpeg.exe"; 
            //ffmpeg -i video.avi -i audio.mp3 -codec copy -shortest output.avi                                                                             // ffmpegProcess.StartInfo.Arguments = " -i " + inputFile + " -vn -f mp3 -ab 320k output " + outputFile;
            ffmpegProcess.StartInfo.Arguments = "-i " + inputFile + " -i " + audioFile + " -codec copy -shortest " + outputFile;
            ffmpegProcess.Start();
            ffmpegProcess.StandardOutput.ReadToEnd();
            mp3out = ffmpegProcess.StandardError.ReadToEnd();
            ffmpegProcess.WaitForExit();
            if (!ffmpegProcess.HasExited)
            {
                ffmpegProcess.Kill();
            }
            Console.WriteLine(mp3out);
            if(File.Exists(outputFile) && AnaglyphParameters.HasSound)
            File.Delete(inputFile);
            File.Delete(audioFile);
            ffmpegProcess.Close();


        }

        public static string GetMetadataFromVideo(string pathToRead)
        {
            Random rnd = new Random();
            int num = rnd.Next(2, 100);

            var inputFile = pathToRead;
            var outputFile = AppDomain.CurrentDomain.BaseDirectory + "metadata.txt";
            var meta = "";
            var ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardInput = true;
            ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.CreateNoWindow = true;
            ffmpegProcess.StartInfo.FileName = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "ffmpeg\\ffmpeg.exe");//"C:\\Users\\Filip\\Source\\Repos\\Picture3D2\\Picture3D2\\ffmpeg-20190323-5fceac1-win32-shared\\bin\\ffmpeg.exe";//@"\ffmpeg-20190323-5fceac1-win32-shared\bin\ffmpeg.exe"; 
            //ffmpeg -i in.mp4 -f ffmetadata in.txt                                                                       // ffmpegProcess.StartInfo.Arguments = " -i " + inputFile + " -vn -f mp3 -ab 320k output " + outputFile;
            ffmpegProcess.StartInfo.Arguments = "-i " + inputFile + " -f ffmetadata " + outputFile;
            ffmpegProcess.Start();
            ffmpegProcess.StandardOutput.ReadToEnd();
            meta = ffmpegProcess.StandardError.ReadToEnd();
            ffmpegProcess.WaitForExit();
            if (!ffmpegProcess.HasExited)
            {
                ffmpegProcess.Kill();
            }
            Console.WriteLine(meta);
            File.Delete(outputFile);
            ffmpegProcess.Close();
            return meta;


        }

        protected virtual void OnOnFrameDone(int frameNum, int totalNum)
        {
            OnFrameDone?.Invoke(this, new NotifyEventArgs { Progress = frameNum, Total = totalNum });
        }

        protected virtual void OnOnProcessDone()
        {
            OnProcessDone?.Invoke(this, EventArgs.Empty);
        }
    }

    public class NotifyEventArgs : EventArgs
    {
        public int Progress;
        public int Total;
    }
}
