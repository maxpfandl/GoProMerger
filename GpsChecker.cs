using System;
using System.Diagnostics;
using Alturos.VideoInfo;

namespace GoProMerger
{
    public class GpsChecker
    {
        private static string _ffprobe = @"c:\Users\S6480\Downloads\ffmpeg-2025-02-17-git-b92577405b-full_build\bin\ffprobe.exe";
        public GpsChecker()
        {
            // Constructor logic here
        }

        public static async Task<bool> HasGpsData(string path)
        {
            var videoAnalyer = new VideoAnalyzer(_ffprobe);
            var analyzeResult = await videoAnalyer.GetVideoInfoAsync(path);
            var videoInfo = analyzeResult.VideoInfo;
            if(videoInfo.Streams.Count() == 4 && videoInfo.Streams[3].Tags.HandlerName == "GoPro MET")
            {
                return true;
            }
            return false;
        }
    }
}