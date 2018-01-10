using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace STB.Tools.SQLToPOCO
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("filename invalid !");

                return;
            }

            var root = Path.Combine(Environment.CurrentDirectory, "src");
            CleanSrcFolder(root);
            var sqlFile = File.ReadAllText(args[0]);
            
            var arrs=Regex.Split(sqlFile,"-- [-]+\n");
            foreach (var block in arrs)
            {
                Console.ReadKey();
                var lines = ReadByLine(block);
                ProcessLines(lines.ToList());
                
            }
            Console.ReadKey();
        }

        private static void CleanSrcFolder(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    File.Delete(file);
                }

                foreach (var directory in Directory.GetDirectories(path))
                {
                    CleanSrcFolder(directory);
                }
                Directory.Delete(path);
            }
            
        }

        private static void ProcessLines(List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (i == 0)
                {
                    CreateFolder(GetPascalName(line));
                }
            }
        }

        private static string GetPascalName(string line)
        {
            if (!line.StartsWith("--")) return null;
            var arr=line.Replace("-", "").Split(" ").Select(c =>
            {
                if (c.Length > 1)
                {
                    if (Char.IsLower(c[0]))
                    {
                        return Char.ToUpper(c[0]) + c.Substring(1, c.Length - 1);
                    }
                }
                return c;
            });
            return string.Join("", arr);
        }

        private static IEnumerable<string> ReadByLine(string block)
        {
            var lines = block.Split("\n");
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    yield return line.Trim();
            }
        }

        static void CreateFolder(string name)
        {
            var root = Path.Combine(Environment.CurrentDirectory, "src");
            var path =
                string.IsNullOrEmpty(name)
                    ? root
                    : Path.Combine(root, name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            Console.WriteLine(path);
        }
    }
}
