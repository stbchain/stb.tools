using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

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
            string lastline = "";
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (i == 0)
                {
                    var ns = GetPascalName(line,true);
                    CreateFolder(ns);
                }

                if (line.StartsWith("CREATE TABLE"))
                {
                  
                    var j = 0;
                    var cls = new ClassObject();
                    while (!line.StartsWith(");"))
                    {
                        line = lines[i];
                        if (line.StartsWith(");"))
                            break;
                        if (j == 0)
                        {
                            Console.WriteLine(line);
                            var name = GetPascalName(line.Replace("CREATE TABLE", ""),false);
                            cls.Name = name;
                            //class name
                            if (lastline.StartsWith("--"))
                            {
                                cls.Comment = lastline.Replace("--", "");
                            }
                        }
                        else
                        {
                            ProcessProperty(cls, line);
                        }

                        j++;
                        i++;
                    }

                    Console.WriteLine(JsonConvert.SerializeObject(cls));
                    Console.ReadKey();
                }
                lastline = line;
               
            }
        }

        private static void ProcessProperty(ClassObject cls, string line)
        {
            if (line.StartsWith("--") ||
                line.StartsWith("PRIMARY KEY") ||
                line.StartsWith("FOREIGN KEY") ||
                line.StartsWith("CONSTRAINT")) return;
            var arr = line.Split(' ');
            var prop = new PropertyObject();
            prop.PropertyName = GetPascalName(arr[0],false);
            try
            {
                prop.RawType = arr[1];
            }
            catch
            {
                Console.WriteLine(line);
                throw;
            }

            if (line.Contains("-- "))
            {
                prop.Comment = line.Split("--", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            }

            if (line.Contains("DEFAULT"))
            {
                prop.DefaultValue = line.Split("DEFAULT", StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?
                    .Replace(",","").Replace("'","");
            }

            if (line.Contains("PRIMARY KEY"))
            {
                prop.IsPrimary = true;
            }

            if (line.Contains("NOT NULL"))
            {
                prop.IsNotNull = true;
            }

            cls.Properties.Add(prop);
        }

        private static string GetPascalName(string line,bool mustMu)
        {
            if (mustMu && !line.StartsWith("--")) return null;
            var arr=line.Replace("-", "").Split(' ', '_').Select(c =>
            {
                if (c.Length > 1)
                {
                    if (c == "mc")
                    {
                        return "MainChain";
                    }
                    if (Char.IsLower(c[0]))
                    {
                        return Char.ToUpper(c[0]) + c.Substring(1, c.Length - 1);
                    }
                }
                return c;
            });
            return string.Join("", arr).Replace("(", "");
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
