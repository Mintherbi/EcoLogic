using NumSharp;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;


namespace PointCloudDiffusion.Utils
{
    public static class Utils
    {
        public static double[,] Point2Array(List<Point3d> lspt)
        {
            int len = lspt.Count;
            double[,] arrpt = new double[len, 3];

            for (int i = 0; i < len; i++)
            {
                arrpt[i, 0] = lspt[i].X;
                arrpt[i, 1] = lspt[i].Y;
                arrpt[i, 2] = lspt[i].Z;
            }

            return arrpt;
        }

        public static List<Point3d> Array2Point(double[,] arrpt, int len)
        {
            List<Point3d> lspt = new List<Point3d>();

            for (int i = 0; i < len; i++)
            {
                lspt.Add(new Point3d(arrpt[i, 0], arrpt[i, 1], arrpt[i, 2]));
            }
            return lspt;
        }

        public static async Task SavePointCloudAsNpy(List<List<Point3d>> pointClouds, string filePath, Action<string> onProgress = null)
        {
            await Task.Run(() =>
            {
                int batchSize = pointClouds.Count;
                int numPoints = pointClouds[0].Count;

                double[,,] data = new double[batchSize, numPoints, 3];

                for (int i = 0; i < batchSize; i++)
                {
                    for (int j = 0; j < numPoints; j++)
                    {
                        data[i, j, 0] = pointClouds[i][j].X;
                        data[i, j, 1] = pointClouds[i][j].Y;
                        data[i, j, 2] = pointClouds[i][j].Z;
                    }

                    if ((i + 1) % 10 == 0)
                    {
                        onProgress?.Invoke($"{i + 1} pointclouds processed");
                    }
                }

                var npArray = np.array(data);
                np.save(filePath, npArray);

                onProgress?.Invoke("Process Completed");
            });

        }

        ///CUDA Functions
        [DllImport(@"C:\Users\jord9\source\repos\Mintherbi\PointCloudDiffusion\x64\Debug\ParallelVectorCalculation.dll")]
        public static extern void VectorAdd(double[,] point1, double[,] point2, int len, double[,] result);

        [DllImport(@"C:\Users\jord9\source\repos\Mintherbi\PointCloudDiffusion\x64\Debug\ParallelVectorCalculation.dll")]
        public static extern void BlockVectorAdd(double[,] point1, double[,] point2, int len, double[,] result);

        /*
        public static List<PythonArg> ParseArguments(string pyFilePath)
        {
            var args = new List<PythonArg>();

            if (!File.Exists(pyFilePath)) return args;

            string code = File.ReadAllText(pyFilePath);

            // ArgumentParser 변수 이름 찾기
            var parserNames = new List<string>();
            var parserInitPattern = new Regex(@"(?<name>\w+)\s*=\s*argparse\.ArgumentParser", RegexOptions.Compiled);
            foreach (Match match in parserInitPattern.Matches(code))
                parserNames.Add(match.Groups["name"].Value);

            if (parserNames.Count == 0)
                parserNames.Add("parser"); // fallback

            // 모든 add_argument(...) 구문을 멀티라인 포함해서 포착
            var argPattern = new Regex(@"(?<parser>\w+)\.add_argument\((?<content>.*?)\)", RegexOptions.Singleline);
            foreach (Match match in argPattern.Matches(code))
            {
                string parserName = match.Groups["parser"].Value;
                string content = match.Groups["content"].Value;

                if (!parserNames.Contains(parserName)) continue;

                // --argname
                var nameMatch = Regex.Match(content, @"['""]--(?<name>[^'""]+)['""]");
                if (!nameMatch.Success) continue;

                var arg = new PythonArg
                {
                    Name = nameMatch.Groups["name"].Value,
                    Type = null,
                    Default = null
                };

                // store_true / store_false
                var storeTrue = Regex.IsMatch(content, @"action\s*=\s*['""]store_true['""]");
                var storeFalse = Regex.IsMatch(content, @"action\s*=\s*['""]store_false['""]");
                if (storeTrue || storeFalse)
                {
                    arg.Type = "bool";
                    arg.Default = storeTrue ? "False" : "True";
                }

                // type
                var typeMatch = Regex.Match(content, @"type\s*=\s*(?<type>\w+)");
                if (typeMatch.Success)
                    arg.Type = typeMatch.Groups["type"].Value;

                // default
                var defaultMatch = Regex.Match(content, @"default\s*=\s*(?<val>[^,\)\n]+)");
                if (defaultMatch.Success)
                    arg.Default = defaultMatch.Groups["val"].Value.Trim();

                // choices
                var choicesMatch = Regex.Match(content, @"choices\s*=\s*\[(?<choices>[^\]]+)\]");
                if (choicesMatch.Success)
                {
                    var choiceStr = choicesMatch.Groups["choices"].Value;
                    arg.Choices = choiceStr.Split(',')
                                           .Select(s => s.Trim(' ', '\'', '"'))
                                           .ToList();
                }

                // type 없으면 default로부터 유추
                if (string.IsNullOrEmpty(arg.Type) && arg.Default != null)
                {
                    string d = arg.Default;
                    if (int.TryParse(d, out _))
                        arg.Type = "int";
                    else if (double.TryParse(d, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        arg.Type = "double";
                    else if (d.Equals("True", StringComparison.OrdinalIgnoreCase) || d.Equals("False", StringComparison.OrdinalIgnoreCase))
                        arg.Type = "bool";
                    else
                        arg.Type = "str";
                }

                // 값 초기화
                arg.Value = ConvertDefault(arg.Type ?? "str", arg.Default);

                args.Add(arg);
            }

            return args;
        }

        */

        public static List<PythonArg> ParseArguments(string pyFilePath)
        {
            var args = new List<PythonArg>();
            if (!File.Exists(pyFilePath)) return args;

            string code = File.ReadAllText(pyFilePath);

            // 1. ArgumentParser 이름 추출 (argparse, exp.argparse 등 모두 허용)
            var parserNames = new List<string>();
            var parserInitPattern = new Regex(@"(?<name>\w+)\s*=\s*(\w+\.)?ArgumentParser", RegexOptions.Compiled);
            foreach (Match match in parserInitPattern.Matches(code))
                parserNames.Add(match.Groups["name"].Value);

            if (parserNames.Count == 0)
                parserNames.Add("parser"); // fallback

            // 2. add_argument(...) 전체 정규식 파싱 (멀티라인 지원)
            var argPattern = new Regex(@"(?<parser>\w+)\.add_argument\((?<content>.*?)\)", RegexOptions.Singleline);
            foreach (Match match in argPattern.Matches(code))
            {
                string parserName = match.Groups["parser"].Value;
                string content = match.Groups["content"].Value;

                if (!parserNames.Contains(parserName)) continue;

                // --name
                var nameMatch = Regex.Match(content, @"['""]--(?<name>[^'""]+)['""]");
                if (!nameMatch.Success) continue;

                var arg = new PythonArg
                {
                    Name = nameMatch.Groups["name"].Value,
                    Type = null,
                    Default = null
                };

                // store_true / store_false
                bool isStoreTrue = Regex.IsMatch(content, @"action\s*=\s*['""]store_true['""]");
                bool isStoreFalse = Regex.IsMatch(content, @"action\s*=\s*['""]store_false['""]");
                if (isStoreTrue || isStoreFalse)
                {
                    arg.Type = "bool";
                    arg.Default = isStoreTrue ? "False" : "True";
                }

                // type
                var typeMatch = Regex.Match(content, @"type\s*=\s*(?<type>\w+)");
                if (typeMatch.Success)
                {
                    string rawType = typeMatch.Groups["type"].Value;
                    arg.Type = rawType == "eval" ? "bool" : rawType;
                }

                // default
                var defaultMatch = Regex.Match(content, @"default\s*=\s*(?<val>[^,\)\n]+)");
                if (defaultMatch.Success)
                    arg.Default = defaultMatch.Groups["val"].Value.Trim();

                // choices
                var choicesMatch = Regex.Match(content, @"choices\s*=\s*\[(?<choices>[^\]]+)\]");
                if (choicesMatch.Success)
                {
                    var choiceStr = choicesMatch.Groups["choices"].Value;
                    arg.Choices = choiceStr.Split(',')
                                           .Select(s => s.Trim(' ', '\'', '"'))
                                           .ToList();
                }

                // type이 없으면 default를 기반으로 추론
                if (string.IsNullOrEmpty(arg.Type) && arg.Default != null)
                {
                    string d = arg.Default;
                    if (int.TryParse(d, out _))
                        arg.Type = "int";
                    else if (double.TryParse(d, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        arg.Type = "double";
                    else if (d.Equals("True", StringComparison.OrdinalIgnoreCase) || d.Equals("False", StringComparison.OrdinalIgnoreCase))
                        arg.Type = "bool";
                    else
                        arg.Type = "str";
                }

                // 값 초기화
                arg.Value = ConvertDefault(arg.Type ?? "str", arg.Default);

                args.Add(arg);
            }

            return args;
        }
        /*
                public static object ConvertDefault(string type, string raw)
                {
                    try
                        {
                            raw = raw.Trim().Replace("THOUSAND", "1000");

                            if (raw.Contains("float('inf'"))
                                return int.MaxValue;

                            switch (type.ToLower())
                            {
                                case "int":
                                    return EvaluateIntExpression(raw);
                                case "float":
                                case "double":
                                    return double.Parse(raw, CultureInfo.InvariantCulture);
                                case "bool":
                                case "eval":
                                    return bool.Parse(raw);
                                default:
                                    return raw.Trim('\"', '\''); // 문자열 따옴표 제거
                            }
                        }
                        catch
                        {
                            return raw;
                        }
                    }

                public static int EvaluateIntExpression(string expression)
                    {
                        try
                        {
                            var dataTable = new System.Data.DataTable();
                            var result = dataTable.Compute(expression, null);
                            return Convert.ToInt32(result);
                        }
                        catch
                        {
                            throw new FormatException($"Unable to evaluate expression: {expression}");
                        }
                    }
        */

        public static object ConvertDefault(string type, string raw)
        {
            if (raw == null) return null;

            raw = raw.Trim().Replace("THOUSAND", "1000");

            if (raw.Contains("float('inf'"))
                return int.MaxValue;

            try
            {
                switch (type.ToLower())
                {
                    case "int":
                        return EvaluateIntExpression(raw);
                    case "float":
                    case "double":
                        return double.Parse(raw, CultureInfo.InvariantCulture);
                    case "bool":
                        return bool.Parse(raw);
                    default:
                        return raw.Trim('\"', '\'');
                }
            }
            catch
            {
                return raw;
            }
        }

        public static int EvaluateIntExpression(string expression)
        {
            try
            {
                var dataTable = new System.Data.DataTable();
                var result = dataTable.Compute(expression, null);
                return Convert.ToInt32(result);
            }
            catch
            {
                throw new FormatException($"Unable to evaluate expression: {expression}");
            }
        }

        public static string ConvertWindowsPathToLinux(string windowsPath)
        {
            if (string.IsNullOrWhiteSpace(windowsPath)) return "";

            string path = windowsPath.Replace("\\", "/"); // 역슬래시 → 슬래시
            if (Regex.IsMatch(path, @"^[a-zA-Z]:"))
            {
                // 드라이브 문자 추출
                char driveLetter = char.ToLower(path[0]);
                path = path.Substring(2); // "C:" 제거
                path = $"/mnt/{driveLetter}{path}";
            }

            return path;
        }

        public static string ToCommandLineArguments(List<PythonArg> args)
        {
            var parts = new List<string>();

            foreach (var arg in args)
            {
                if (arg.Value == null)
                    continue;

                string key = $"--{arg.Name}";
                string val;

                if (arg.Value is bool b)
                {
                    val = b ? "True" : "False"; // Python 쪽 eval 타입 대응
                }
                else if (arg.Value is List<string> list)
                {
                    val = string.Join(" ", list.Select(s => s.Contains(' ') ? $"\"{s}\"" : s));
                }
                else if (arg.Value is double d)
                {
                    val = d.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    val = arg.Value.ToString();
                }

                parts.Add($"{key} {val}");
            }

            return string.Join(" ", parts);
        }
    }


}

    public class PythonArg
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Default { get; set; }
        public List<string> Choices { get; set; } = new();
        public object Value { get; set; }

    }


