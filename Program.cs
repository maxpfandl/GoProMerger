using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace MyApp
{

    internal class Program
    {
        static string _ffmpegPath = @"c:\Program Files\ffmpeg\bin\ffmpeg.exe";
        static bool _nogeo = false;
        static bool _delete = false;
        static void Main(string[] args)
        {

            string[] files = new string[0];
            if (args.Length == 0)
            {
                files = Directory.GetFiles(Environment.CurrentDirectory, "*.mp4");
            }
            else
            {
                if (args[0] == "nogeo" || args[0] == "delete")
                {
                    if (args[0] == "nogeo")
                        _nogeo = true;
                    if (args[0] == "delete")
                        _delete = true;

                    if (args.Length == 2)
                    {
                        if (args[1] == "nogeo")
                            _nogeo = true;
                        if (args[1] == "delete")
                            _delete = true;
                    }

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

            List<string> filesList = files.ToList();
            filesList.Sort();


            List<List<string>> filesLists = new List<List<string>>();
            List<string> matches = new List<string>();

            Regex regexTime = new Regex(@"^\\d{6}_G\\w\\d\{6\}.mp4", RegexOptions.IgnoreCase);
            Regex regexOrig = new Regex(@"^G\\w\\d\{6\}.mp4", RegexOptions.IgnoreCase);

            foreach (var file in filesList)
            {

                //remove date from filename
                var filename = Path.GetFileName(file);
                if (regexTime.IsMatch(filename))
                {
                    var name = filename.Remove(0, 9);
                    if (name.StartsWith("01"))
                    {
                        if (matches.Count > 1)
                        {
                            filesLists.Add(matches);
                        }
                        matches = new List<string>();
                        matches.Add(file);
                    }
                    else
                    {
                        matches.Add(file);
                    }
                }

            }

            if (matches.Count > 1)
            {
                filesLists.Add(matches);
            }


            if (filesLists.Count == 0)
            {
                Console.WriteLine("No filegroups found (ie 105545_GX010001.mp4, 113045_GX020001.mp4)");
                return;
            }

            foreach (var fileList in filesLists)
            {

                Console.WriteLine($"===={Environment.NewLine}New Group:");
                foreach (var file in fileList)
                {
                    Console.WriteLine(file);
                }
                if (fileList.Count <= 1)
                {
                    continue;
                }

                List<string> concatFiles = new List<string>();
                foreach (var file in fileList)
                {
                    concatFiles.Add($"file '{file}'");
                }
                concatFiles.Sort();

                foreach (var file in concatFiles)
                {
                    Console.WriteLine(file);
                }

                var inputFiles = Path.GetTempFileName();
                File.WriteAllLines(inputFiles, concatFiles);


                string outputFile = Path.Combine(Path.GetDirectoryName(fileList[0])!, Path.GetFileNameWithoutExtension(fileList[0]) + "_concat.mp4");
                Console.WriteLine($"Output: {outputFile}");

                try
                {

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = _ffmpegPath;

                    // ffmpeg -y -f concat -safe 0 -i test.txt -c copy -copy_unknown -map 0:v -map 0:a -map 0:2 -map 0:3 -map 0:4 -tag:2 tmcd -tag:3 gpmd -tag:4 fdsc test2.mp4
                    if (_nogeo)
                    {
                        psi.Arguments = $"-hide_banner -loglevel error -y -f concat -safe 0 -i \"{inputFiles}\" -c copy -map 0:v -map 0:a  \"{outputFile}\"";
                    }
                    else
                    {
                        psi.Arguments = $"-hide_banner -loglevel error -y -f concat -safe 0 -i \"{inputFiles}\" -c copy -map 0:v -map 0:a -map 0:3 -copy_unknown -tag:2 gpmd  \"{outputFile}\"";
                    }

                    // Console.WriteLine(psi.Arguments);
                    // return;
                    // psi.Arguments = $"-n -loglevel error -hwaccel cuda -stats -i \"{file}\" -c:v libx265 -c:a aac \"{tmpfile}\"";

                    Process proc = new Process
                    {
                        StartInfo = psi
                    };
                    var myproc = proc.Start();
                    proc.WaitForExit();
                    if (_delete && proc.ExitCode == 0)
                    {
                        foreach (var file in fileList)
                        {
                            Console.WriteLine($"Deleting {file}");
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    File.Delete(inputFiles);
                }

            }

        }
    }
}