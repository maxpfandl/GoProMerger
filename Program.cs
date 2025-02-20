using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoProMerger
{

    internal class Program
    {
        static string _ffmpegPath = @"c:\Program Files\ffmpeg\bin\ffmpeg.exe";
        static bool _nogeo = false;
        static bool _nogroup = false;
        static bool _delete = false;
        static async Task Main(string[] args)
        {


            string[] files = new string[0];
            if (args.Length == 0)
            {
                files = Directory.GetFiles(Environment.CurrentDirectory, "*.mp4");
            }
            else
            {

                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        if (arg == "nogeo")
                        {
                            _nogeo = true;
                        }
                        else if (arg == "delete")
                        {
                            _delete = true;
                        }
                        else if (arg == "nogroup")
                        {
                            _nogroup = true;
                        }
                        else if (Directory.Exists(arg))
                        {
                            files = Directory.GetFiles(arg, "*.mp4");
                        }
                        else
                        {
                            Console.WriteLine("GoProMerger [nogeo] [delete] [nogroup] [directory]");
                            return;
                        }

                    }
                }

                if (files.Length == 0)
                {
                    files = Directory.GetFiles(Environment.CurrentDirectory, "*.mp4");
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
            Dictionary<string, List<string>> matches = new Dictionary<string, List<string>>();

            Regex regexTime = new Regex(@"^\d{6}_G\w\d{6}.mp4", RegexOptions.IgnoreCase);
            Regex regexOrig = new Regex(@"^G\w\d{6}.mp4", RegexOptions.IgnoreCase);

            if (_nogroup)
            {
                filesLists.Add(filesList);
            }
            else
            {
                foreach (var file in filesList)
                {

                    //remove date from filename
                    var filename = Path.GetFileName(file);
                    var name = "";
                    if (regexTime.IsMatch(filename))
                    {
                        name = filename.Remove(0, 11);

                    }
                    else if (regexOrig.IsMatch(filename))
                    {
                        name = filename.Remove(0, 4);
                    }
                    else
                    {
                        Console.WriteLine($"Skipping {filename}");
                        continue;
                    }

                    if (matches.ContainsKey(name))
                    {
                        matches[name].Add(file);
                    }
                    else
                    {
                        matches.Add(name, new List<string>() { file });
                    }

                }

                foreach (var match in matches)
                {
                    if (match.Value.Count > 1)
                    {
                        filesLists.Add(match.Value);
                    }
                }
            }




            if (filesLists.Count == 0 && !_nogroup)
            {
                Console.WriteLine("No filegroups found (ie 105545_GX010001.mp4, 113045_GX020001.mp4 or GX010001.mp4, GX020001.mp4)");
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


                var inputFiles = Path.GetTempFileName();
                File.WriteAllLines(inputFiles, concatFiles);


                string outputFile = Path.Combine(Path.GetDirectoryName(fileList[0])!, Path.GetFileNameWithoutExtension(fileList[0]) + "_concat.mp4");
                Console.WriteLine($"Output: {outputFile}");

                try
                {


                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = _ffmpegPath;

                    // ffmpeg -y -f concat -safe 0 -i test.txt -c copy -copy_unknown -map 0:v -map 0:a -map 0:2 -map 0:3 -map 0:4 -tag:2 tmcd -tag:3 gpmd -tag:4 fdsc test2.mp4
                    if (_nogeo || !await GpsChecker.HasGpsData(fileList[0]))
                    {
                        psi.Arguments = $"-hide_banner -loglevel error -y -f concat -safe 0 -i \"{inputFiles}\" -c copy -map 0:v -map 0:a  \"{outputFile}\"";
                    }
                    else
                    {
                        psi.Arguments = $"-hide_banner -loglevel error -y -f concat -safe 0 -i \"{inputFiles}\" -c copy -map 0:v -map 0:a -map 0:3 -copy_unknown -tag:2 gpmd  \"{outputFile}\"";
                    }

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