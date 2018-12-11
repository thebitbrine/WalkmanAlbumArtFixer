using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalkmanAlbumArtFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "WAAF v1.0";
            Console.WriteLine(@" __      __  _____     _____ ___________");
            Console.WriteLine(@"/  \    /  \/  _  \   /  _  \\_   _____/");
            Console.WriteLine(@"\   \/\/   /  /_\  \ /  /_\  \|    __)  ");
            Console.WriteLine(@" \        /    |    /    |    |     \   ");
            Console.WriteLine(@"  \__/\  /\____|__  \____|__  \___  /   ");
            Console.WriteLine(@"       \/         \/        \/    \/    ");
            Console.WriteLine(@"     Walkman Album Art Fixer v1.0       ");
            Console.WriteLine(@"========================================");
            Console.WriteLine(@"=   thebitbrine/WalkmanAlbumArtFixer   =");
            Console.WriteLine(@"========================================");

            Program p = new Program();
            p.Run();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public void Run()
        {
            Print("Enter path: ",false);
            string Home = Console.ReadLine();

            Print("Enter extentions (i.e mp3, flac): ", false);
            string Extentions = Console.ReadLine();
            string[] ExtentionsArray = { "mp3", "flac", "m4a" };
            if(!string.IsNullOrWhiteSpace(Extentions)) ExtentionsArray = Extentions.ToLower().Replace(" ","").Replace(".","").Split(',');

            List<string> AllFiles = ListAllFiles(Home);
            List<string> MP3Files = new List<string>();
            foreach (var File in AllFiles)
            {
                foreach (var Extention in ExtentionsArray)
                {
                    if (File.ToLower().EndsWith("." + Extention))
                        MP3Files.Add(File);
                }
            }

            List<string> FailedToFix = new List<string>();
            foreach (var MP3 in MP3Files)
            {
                try
                {
                    var TagFile = TagLib.File.Create(MP3);
                    var Pictures = TagFile.Tag.Pictures;
                    string NewPath = MP3.Remove(MP3.LastIndexOf('.')) + $".waaf.jpg";
                    foreach (var Picture in Pictures)
                    {
                        File.WriteAllBytes(NewPath, Picture.Data.Data);
                        string response = SingleExecute(Rooter("bin/jpegtran.exe"), "-o \"" + NewPath + "\" \"" + NewPath + "\"", true);
                        if (response != "") FailedToFix.Add(MP3);
                        byte[] FixedCover = File.ReadAllBytes(NewPath);
                        Picture.Data = new TagLib.ByteVector(FixedCover, FixedCover.Length);
                    }

                    try { TagFile.Save(); PrintLine("Fixed " + MP3.Replace("\\", "/").Split('/').Last()); }
                    catch (Exception ex) { FailedToFix.Add(MP3); PrintLine("Failed to fix " + MP3.Replace("\\", "/").Split('/').Last()); PrintLine("Reason: " + ex.Message); }
                    int index = MP3Files.FindIndex(a => a == MP3) + 1;
                    Console.Title = "WAAF: " + index + "/" + MP3Files.Count + $" ({Math.Round(((double)index / MP3Files.Count) * 100, 2)}%)";
                    File.Delete(NewPath);
                }
                catch(Exception ex) { FailedToFix.Add(MP3); PrintLine("Failed to fix " + MP3.Replace("\\", "/").Split('/').Last()); PrintLine("Reason: " + ex.Message); }
            }
            Console.Clear();
            Print("Process finished ");
            if (FailedToFix.Count > 0)
                PrintLine("with some problems: ");
            else
                PrintLine("with no problems.");
            foreach (var Fail in FailedToFix)
            {
                Console.WriteLine("Failed to fix: " + Fail);
            }
            
        }

        public List<string> ListAllFiles(string FullPath)
        {
            List<string> Files = new List<string>();
            try
            {
                foreach (string File in System.IO.Directory.GetFiles(FullPath))
                {
                    Files.Add(File);
                }
                foreach (string Directory in System.IO.Directory.GetDirectories(FullPath))
                {
                    Files.AddRange(ListAllFiles(Directory));
                }
            }
            catch (System.Exception e)
            {
                //Handle Exception
            }
            return Files;
        }

        #region Essentials
        public string LogPath = @"data\Logs.txt";
        public bool NoConsolePrint = false;
        public bool NoFilePrint = false;
        public void Print(string String)
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "")));
            if (!NoConsolePrint) Console.Write(Tag(String));
        }
        public void Print(string String, bool DoTag)
        {
            Check();
            if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", ""))); if (!NoConsolePrint) Console.Write(Tag(String)); }
            else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "")); if (!NoConsolePrint) Console.Write(String); }
        }
        public void PrintLine(string String)
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine));
            if (!NoConsolePrint) Console.WriteLine(Tag(String));
        }
        public void PrintLine(string String, bool DoTag)
        {
            Check();
            if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine)); if (!NoConsolePrint) Console.WriteLine(Tag(String)); }
            else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "") + Environment.NewLine); if (!NoConsolePrint) Console.WriteLine(String); }
        }
        public void PrintLine()
        {
            Check();
            if (!NoFilePrint) WaitWrite(Rooter(LogPath), Environment.NewLine);
            if (!NoConsolePrint) Console.WriteLine();
        }
        public void PrintLines(string[] StringArray)
        {
            Check();
            foreach (string String in StringArray)
            {
                if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine));
                if (!NoConsolePrint) Console.WriteLine(Tag(String));
            }
        }
        public void PrintLines(string[] StringArray, bool DoTag)
        {
            Check();
            foreach (string String in StringArray)
            {
                if (DoTag) { if (!NoFilePrint) WaitWrite(Rooter(LogPath), Tag(String.Replace("\r", "") + Environment.NewLine)); if (!NoConsolePrint) Console.WriteLine(Tag(String)); }
                else { if (!NoFilePrint) WaitWrite(Rooter(LogPath), String.Replace("\r", "") + Environment.NewLine); if (!NoConsolePrint) Console.WriteLine(String); }
            }
        }
        public void Check()
        {
            if (!NoFilePrint && !System.IO.File.Exists(LogPath)) Touch(LogPath);
        }
        private bool WriteLock = false;
        public void WaitWrite(string Path, string Data)
        {
            while (WriteLock) { System.Threading.Thread.Sleep(20); }
            WriteLock = true;
            System.IO.File.AppendAllText(Path, Data);
            WriteLock = false;
        }
        public string[] ReadData(string DataDir)
        {
            if (System.IO.File.Exists(DataDir))
            {
                List<string> Data = System.IO.File.ReadAllLines(DataDir).ToList<string>();
                foreach (var Line in Data)
                {
                    if (Line == "\n" || Line == "\r" || Line == "\t" || string.IsNullOrWhiteSpace(Line))
                        Data.Remove(Line);
                }
                return Data.ToArray();
            }
            else
                return null;
        }
        public string ReadText(string TextDir)
        {
            if (System.IO.File.Exists(TextDir))
            {
                return System.IO.File.ReadAllText(TextDir);
            }
            return null;
        }
        public string SafeJoin(string[] Array)
        {
            if (Array != null && Array.Length != 0)
                return string.Join("\r\n", Array);
            else return "";
        }
        public void CleanLine()
        {
            Console.Write("\r");
            for (int i = 0; i < Console.WindowWidth - 1; i++) Console.Write(" ");
            Console.Write("\r");
        }
        public void CleanLastLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            CleanLine();
        }
        public string Rooter(string RelPath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), RelPath);
        }
        public static string StaticRooter(string RelPath)
        {
            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), RelPath);
        }
        public string Tag(string Text)
        {
            return "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] " + Text;
        }
        public string Tag()
        {
            return "[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "] ";
        }
        public bool Touch(string Path)
        {
            try
            {
                System.Text.StringBuilder PathCheck = new System.Text.StringBuilder();
                string[] Direcories = Path.Split(System.IO.Path.DirectorySeparatorChar);
                foreach (var Directory in Direcories)
                {
                    PathCheck.Append(Directory);
                    string InnerPath = PathCheck.ToString();
                    if (System.IO.Path.HasExtension(InnerPath) == false)
                    {
                        PathCheck.Append("\\");
                        if (System.IO.Directory.Exists(InnerPath) == false) System.IO.Directory.CreateDirectory(InnerPath);
                    }
                    else
                    {
                        System.IO.File.WriteAllText(InnerPath, "");
                    }
                }
                if (IsDirectory(Path) && System.IO.Directory.Exists(PathCheck.ToString())) { return true; }
                if (!IsDirectory(Path) && System.IO.File.Exists(PathCheck.ToString())) { return true; }
            }
            catch (Exception ex) { PrintLine("ERROR: Failed touching \"" + Path + "\". " + ex.Message, true); }
            return false;
        }
        public bool IsDirectory(string Path)
        {
            try
            {
                System.IO.FileAttributes attr = System.IO.File.GetAttributes(Path);
                if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                    return true;
                else
                    return false;
            }
            catch
            {
                if (System.IO.Path.HasExtension(Path)) return true;
                else return false;
            }
        }
        #endregion
        #region Execute
        public bool ExeLogToFile = false;
        public bool ExeLogToConsole = true;
        public string ExeLogPath = StaticRooter("data/exelogs.txt");
        public void Execute(string Executable, string Arguments, bool WaitForExit)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = Executable;
            process.StartInfo.Arguments = Arguments;
            process.StartInfo.WorkingDirectory = Executable.Remove(Executable.LastIndexOf('/'));
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(ExeOutputHandler);
            process.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(ExeOutputHandler);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            if (WaitForExit) process.WaitForExit();
        }
        public string SingleExecute(string Executable, string Arguments, bool WaitForExit, bool RunInCMD = false)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = Executable;
            process.StartInfo.Arguments = Arguments;
            process.StartInfo.WorkingDirectory = Executable.Remove(Executable.LastIndexOf('/'));
            process.StartInfo.WindowStyle = (RunInCMD ? System.Diagnostics.ProcessWindowStyle.Normal : System.Diagnostics.ProcessWindowStyle.Hidden);
            process.StartInfo.UseShellExecute = RunInCMD;
            process.StartInfo.UseShellExecute = RunInCMD;
            process.StartInfo.RedirectStandardOutput = !RunInCMD;
            process.StartInfo.RedirectStandardError = !RunInCMD;
            process.Start();
            if (WaitForExit) process.WaitForExit();
            if (RunInCMD) return "";
            string std = process.StandardOutput.ReadToEnd();
            string etd = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(std)) return std;
            if (!string.IsNullOrWhiteSpace(etd)) return etd;
            return "";
        }

        public void ExeOutputHandler(object sendingProcess, System.Diagnostics.DataReceivedEventArgs outLine)
        {
            string Output = outLine.Data;
            if (string.IsNullOrWhiteSpace(Output) == false)
            {
                if (ExeLogToFile) System.IO.File.AppendAllText(ExeLogPath, Tag(Output + Environment.NewLine));
                if (ExeLogToConsole) Console.WriteLine(Tag(Output));
            }
        }
        #endregion
    }
}
