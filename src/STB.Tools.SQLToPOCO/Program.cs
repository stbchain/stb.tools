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
                //Console.ReadKey();
                var lines = ReadByLine(block);
                ProcessLines(lines.ToList());
                
            }
            //Console.ReadKey();
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
            string ns = "", folder = "";
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (i == 0)
                {
                    ns = GetPascalName(line,true);
                    folder = CreateFolder(ns);
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
                            cls.Namespace = ns;
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

                    WriteClassFile(folder, cls);
                    
                    //Console.ReadKey();
                }
                lastline = line;
               
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="cls"></param>
        private static void WriteClassFile(string folder, ClassObject cls)
        {
            var filename = Path.Combine(folder, $"{cls.Name}.cs");
            if (File.Exists(filename)) return;
            var sb = new StringBuilder();
            sb.Append(@"/*---------------------------------------------------------------------------------------------
 * Copyright (c) STB Chain. All rights reserved.
 * Licensed under the Source EULA. See License in the project root for license information.
 * Source code : https://github.com/stbchain
 * Website : http://www.soft2b.com/
 *---------------------------------------------------------------------------------------------
 ---------------------------------------------------------------------------------------------*/
using System;

namespace STB.Core");
            if (!string.IsNullOrWhiteSpace(cls.Namespace))
            {
                sb.Append($".{cls.Namespace}");
            }

            sb.AppendLine().AppendLine("{");//begin ns
            if (!string.IsNullOrWhiteSpace(cls.Comment))
            {
                sb.AppendLine("\t/// <summary>");
                sb.AppendLine($"\t/// {cls.Comment}");
                sb.AppendLine("\t/// </summary>");
            }
            sb.AppendLine($"\tpublic class {cls.Name}");
            sb.AppendLine("\t{");//begin class
            foreach (var prop in cls.Properties)
            {
                if (string.IsNullOrWhiteSpace(prop.PropertyName)) continue;
                if (!string.IsNullOrWhiteSpace(prop.Comment))
                {
                    sb.AppendLine("\t\t/// <summary>");
                    sb.AppendLine($"\t\t/// {prop.Comment}");
                    sb.AppendLine("\t\t/// </summary>");
                }
                sb.Append($"\t\tpublic {prop.PropertyType} {prop.PropertyName} {{get;set;}}");
                if (prop.DefaultValue != null)
                {
                    sb.Append($" = {prop.DefaultValue};");
                }

                sb.AppendLine();
            }
            sb.AppendLine("\t}");//close class
            sb.AppendLine("}");//close namespace
            Console.WriteLine(sb.ToString());
            File.WriteAllText(filename, sb.ToString());

        }

        private static void ProcessProperty(ClassObject cls, string line)
        {
            var skipStarts = new[]
            {
                "--",
                "UNIQUE",
                "PRIMARY KEY",
                "FOREIGN KEY",
                "CONSTRAINT"
            };
            if (skipStarts.Any(line.StartsWith)) return;
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

        static string CreateFolder(string name)
        {
            var root = Path.Combine(Environment.CurrentDirectory, "src");
            var path =
                string.IsNullOrEmpty(name)
                    ? root
                    : Path.Combine(root, name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}
