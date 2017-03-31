using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseFolders
{
    class Program
    {
        static void Main(string[] args)
        {

            string savePath = "result.txt";
            if ((args.Length > 2) && !File.Exists(args[2]))
            {
                using (File.Create(args[2]))
                {
                    savePath = args[2];
                }
            }
            switch (args[1])
            {
                case "all":
                    All all = new All(args[0], savePath);
                    var res = all.GetAll(args[0]);
                    all.SaveResult(res);
                    break;
                case "reversed1":
                    Reversed reversed = new Reversed(args[0], savePath);
                    reversed.SaveResult(reversed.GetReversed1(args[0]));
                    break;
                case "reversed2":
                    reversed = new Reversed(args[0], savePath);
                    reversed.SaveResult(reversed.GetReversed2(args[0]));
                    break;
                default:
                    Pattern pattern = new Pattern(args[0], args[1], savePath);
                    pattern.SaveResult(pattern.GetAllWithPattern(args[0]));
                    break;
            }
            Console.ReadLine();
        }
    }

    public interface IParseForlder
    {
        void ReturnResult(string path);
        void SaveResult(Task<List<string>> result);
        Task<string[]> GetFolders(string path);   
    }
    public class MainClass : IParseForlder
    {
        private string _Path = "";
        private string _ResultFilePath = "";

        public string StartPath { get { return _Path; } set { _Path = value; } }
        public string ResultFilePath { get { return _ResultFilePath; } set { _ResultFilePath = value; } }

        public MainClass(string startPath, string savePath = "result.txt")
        {
            StartPath = startPath;
            ResultFilePath = savePath;
        }
        public void ReturnResult(string path)
        {
            Console.WriteLine("\t File '{0}'", path);
        }
        public void SaveResult(Task<List<string>> result)
        {
            try
            {
                using (FileStream fs = File.Create(_ResultFilePath))
                {
                    foreach (var i in result.Result)
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(i + "\r\n");
                        fs.Write(info, 0, info.Length);
                    }
                }
                Console.Write("File saved. Path to file: {0}", _ResultFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0} ", ex.Message);
            }
        }
        public Task<string[]> GetFolders(string path)
        {
            string[] result = new string[] { };
            return Task.Run(() =>
            {
                result = Directory.GetDirectories(path);
                foreach (var i in result)
                {
                    GetFolders(i);
                    Task.Delay(1);
                }
                return Directory.GetDirectories(path);
            });
        }
        List<string> result = new List<string>();
    }
    public class All : MainClass
    {
        public All(string startPath, string savePath) : base(startPath, savePath)
        {
        }
        List <string> result = new List<string>();
        public async Task<List<string>> GetAll(string path)
        {
            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                result.Add(Path.GetFileName(fileName));
                ReturnResult(fileName.Replace(StartPath, String.Empty));
            }

            string[] subdirectoryEntries = await GetFolders(path);
            foreach (string subdirectory in subdirectoryEntries)
                await GetAll(subdirectory);
            return result;
        }
    }
    public class Pattern : MainClass
    {
        private string _pattern = "";
        private List<string> result = new List<string>();
        public Pattern(string startPath, string pattern, string savePath) : base (startPath, savePath)
        {
            _pattern = (pattern != String.Empty) ? pattern : "txt";
        }
        public async Task<List<string>> GetAllWithPattern(string path)
        {
            Console.WriteLine("Folder {0}", path);
            string[] fileEntries = Directory.GetFiles(path, "*." + _pattern);
            foreach (string fileName in fileEntries)
            {
                ReturnResult(fileName.Replace(StartPath, String.Empty));
                result.Add(Path.GetFileName(fileName) + "/");
            }

            string[] subdirectoryEntries = await GetFolders(path);
            foreach (string subdirectory in subdirectoryEntries)
              await   GetAllWithPattern(subdirectory);

            return result;
        }
    }
    public class Reversed : MainClass
    {
        private List<string> result = new List<string>();
        public Reversed(string startPath, string savePath) : base (startPath, savePath)
        {

        }
        public async Task<List<string>> GetReversed1(string path)
        {
            Console.WriteLine("Folder {0}", path);
            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                result.Add(Path.GetFileName(fileName));
                string[] mass = fileName.Replace(StartPath, String.Empty).Split('/', '\\');
                string pathExit = "";
                for (int i = mass.Length - 1; i >= 0; i--)
                {
                    pathExit += mass[i];
                    if (i - 1 > 0)
                    {
                        pathExit += @"\";
                    }
                }
                ReturnResult(pathExit);
            }

            string[] subdirectoryEntries = await GetFolders(path); 
            foreach (string subdirectory in subdirectoryEntries)
                await GetReversed1(subdirectory);

            return result;
        }
        public async Task<List<string>> GetReversed2(string path)
        {
          
            Console.WriteLine("Folder {0}", path);
            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                result.Add(Path.GetFileName(fileName));
                char[] mass = fileName.Replace(StartPath, String.Empty).ToCharArray();
                Array.Reverse(mass);
                ReturnResult(new string(mass));
            }

            string[] subdirectoryEntries = await GetFolders(path); ;
            foreach (string subdirectory in subdirectoryEntries)
               await  GetReversed2(subdirectory);

            return result;
        }

    }

}
