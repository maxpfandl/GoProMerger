using System;
using System.Diagnostics;

namespace MyApp
{

    internal class Program
    {
        static string _ffmpegPath = @"c:\Program Files\ffmpeg\bin\ffmpeg.exe";
        static void Main(string[] args)
        {

            var files = Directory.GetFiles(args[0], "*.mp4");

            if (files.Length == 0)
            {
                Console.WriteLine("No files found");
                return;
            }

            List<string> filesList = new List<string>();
            foreach (var file in files)
            {
                filesList.Add($"file '{file}'");
            }
            var inputFiles = Path.GetTempFileName();
            File.WriteAllLines(inputFiles, filesList);
            

            string outputFile = Path.Combine(Path.GetDirectoryName(files[0])!, "_concat-" + Path.GetFileName(files[0]));



            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = _ffmpegPath;
            psi.Arguments = $"-f concat -safe 0 -i \"{inputFiles}\" -c copy \"{outputFile}\"";
            // Console.WriteLine(psi.Arguments);
            // return;
            // psi.Arguments = $"-n -loglevel error -hwaccel cuda -stats -i \"{file}\" -c:v libx265 -c:a aac \"{tmpfile}\"";
            Process proc = new Process
            {
                StartInfo = psi
            };
            proc.Start();
            proc.WaitForExit();
            File.Delete(inputFiles);
        }
    }
}