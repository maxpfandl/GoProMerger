using System;
using System.Diagnostics;
using Alturos.VideoInfo;

namespace GoProMerger
{
    public class GpsChecker
    {
        private static string _ffprobe = @"c:\Program Files\ffmpeg\bin\ffprobe.exe";
        public GpsChecker()
        {
            // Constructor logic here
        }

        public static async Task<bool> HasGpsData(string path)
        {
            var videoAnalyer = new VideoAnalyzer(_ffprobe);
            var analyzeResult = await videoAnalyer.GetVideoInfoAsync(path);
            var videoInfo = analyzeResult.VideoInfo;
            if (videoInfo.Streams.Count() == 4 && videoInfo.Streams[3].Tags.HandlerName == "GoPro MET")
            {
                return true;
            }
            else if (videoInfo.Streams.Count() == 3 && videoInfo.Streams[2].Tags.HandlerName == "GoPro MET")
            {
                return true;
            }
            return false;
        }
    }
}