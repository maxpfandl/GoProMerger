using System;
using System.Diagnostics;

namespace MyApp
{

    internal class Program
    {
        static string _ffmpegPath = @"c:\Program Files\ffmpeg\bin\ffmpeg.exe";
        static bool _nogeo = false;
        static void Main(string[] args)
        {

            string[] files = new string[0];
            if (args.Length == 0)
            {
                files = Directory.GetFiles(Environment.CurrentDirectory, "*.mp4");
            }
            else
            {
                if (args[0] == "nogeo")
                {
                    _nogeo = true;
                    files = Directory.GetFiles(Environment.CurrentDirectory, "*.mp4");
                }
                else if (Directory.Exists(args[0]))
                {
                    files = Directory.GetFiles(args[0], "*.mp4");
                }

            }

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
            filesList.Sort();
            
            foreach(var file in filesList)
            {
                Console.WriteLine(file);
            }

            var inputFiles = Path.GetTempFileName();
            File.WriteAllLines(inputFiles, filesList);


            string outputFile = Path.Combine(Path.GetDirectoryName(files[0])!, "_concat-" + Path.GetFileName(files[0]));



            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = _ffmpegPath;

            // ffmpeg -y -f concat -safe 0 -i test.txt -c copy -copy_unknown -map 0:v -map 0:a -map 0:2 -map 0:3 -map 0:4 -tag:2 tmcd -tag:3 gpmd -tag:4 fdsc test2.mp4
            if (_nogeo)
            {
                psi.Arguments = $"-y -f concat -safe 0 -i \"{inputFiles}\" -c copy -map 0:v -map 0:a  \"{outputFile}\"";
            }
            else
            {
                psi.Arguments = $"-y -f concat -safe 0 -i \"{inputFiles}\" -c copy -map 0:v -map 0:a -map 0:3 -copy_unknown -tag:2 gpmd  \"{outputFile}\"";
            }

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