using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ClangAnalyze
{
    public class Analyzer
    {
        // clang 形式から visual studio形式へ変換用.
        static readonly System.Text.RegularExpressions.Regex LineRegex =
                new System.Text.RegularExpressions.Regex(
                    @"([A-Z]:[\\/].*?):([0-9]*):([0-9]*)(:.*)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        // -Whogehoge->Disable with -Wno-hogehogeへ変換用.
        static readonly System.Text.RegularExpressions.Regex WarningRegex =
                new System.Text.RegularExpressions.Regex(
                    @"(.*\[)(-W)(.*\])",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        // コマンドエラー.
        static readonly System.Text.RegularExpressions.Regex CommandErrorRegex =
                new System.Text.RegularExpressions.Regex(
                    @"error:.*",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public delegate void ProgressMethod(float progress_value);
        /// <summary>
        /// 解析.
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        static public void Analyze(AnalyzeSetting setting, AnalyzeResultNode root, ProgressMethod progress_method = null)
        {
            // 弾いてるけど念のため.
            if (!System.IO.Directory.Exists(setting.AnalyzeDirectory))
            {
                return;
            }

            string command_name = "clang/clang++.exe";
            string arguments = "-cc1";
            arguments += " -analyze";
            arguments += " -analyzer-checker=cplusplus,alpha.cplusplus,core,unix.Malloc,unix.MismatchedDeallocator";
            arguments += " -fdiagnostics-show-option";
            arguments += " -Wall";
            arguments += " -Wno-unused-command-line-argument";
            string directory = setting.AnalyzeDirectory.Replace('\\', '/');
            List<string> result = new List<string>();

            string[] files = System.IO.Directory.GetFiles(setting.AnalyzeDirectory, "*.cpp", System.IO.SearchOption.AllDirectories);

            int max_progress = files.Length * setting.Profiles.Count;
            int finish_count = 0;
            foreach (Profile profile in setting.Profiles)
            {
                string arguments_with_profile = arguments;
                foreach (string option in profile.Options)
                {
                    arguments_with_profile += " " + option;
                }
                foreach (string file_name in files)
                {
                    // プロセスの設定.
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.FileName = command_name;
                    info.Arguments = arguments_with_profile + " " + file_name;
                    info.CreateNoWindow = true;
                    info.UseShellExecute = false;
                    info.RedirectStandardOutput = true;
                    info.RedirectStandardError = true;
                    // プロセスの実行.
                    Process process = Process.Start(info);
                    string std_output = process.StandardOutput.ReadToEnd();
                    string std_error = process.StandardError.ReadToEnd();
                    string process_result = std_output + std_error;

                    if (process_result != "")
                    {
                        process_result = process_result.Replace("\r\r\n", "\n");
                        process_result = process_result.Replace("\r\n", "\n");
                        string[] lines = process_result.Split('\n');

                        foreach (string line in lines)
                        {
                            System.Text.RegularExpressions.Match line_match = LineRegex.Match(line);
                            if (line_match.Groups.Count >= 5)
                            {
                                // /に変換しとく.
                                string source_file_name = line_match.Groups[1].Value.Replace("\\", "/");
                                source_file_name = source_file_name.Replace("\\\\", "/");

                                string new_diagnostics = source_file_name + "(" + line_match.Groups[2].Value + "," + line_match.Groups[3].Value + ") " + line_match.Groups[4].Value;
                                bool is_error = new_diagnostics.Contains("error:");
                                if (!is_error)
                                {
                                    System.Text.RegularExpressions.Match warning_match = WarningRegex.Match(new_diagnostics);
                                    if (warning_match.Groups.Count >= 4)
                                    {
                                        new_diagnostics = warning_match.Groups[1] + "Disable with -Wno-" + warning_match.Groups[3];
                                    }
                                }

                                // 新規エラーorワーニングなら追加.
                                if (!result.Contains(new_diagnostics))
                                {
                                    FileNode file_node = SearchFileNode(root, source_file_name);
                                    if (file_node == null)
                                    {
                                        file_node = new FileNode(source_file_name);
                                        FolderNode parent = SearchFolderNode(root, file_node.DirectoryName);
                                        if (parent == null)
                                        {
                                            parent = MakeFolderNode(root, file_node.DirectoryName);
                                        }
                                        parent.Children.Add(file_node);
                                    }
                                    int line_no = int.Parse(line_match.Groups[2].Value);
                                    int column_no = int.Parse(line_match.Groups[3].Value);
                                    if (is_error)
                                    {
                                        AnalyzeResultNode error = new ErrorNode(source_file_name,
                                            line_no, column_no, new_diagnostics);
                                        file_node.Children.Add(error);
                                    }
                                    else
                                    {
                                        AnalyzeResultNode error = new WarningNode(source_file_name,
                                            line_no, column_no, new_diagnostics);
                                        file_node.Children.Add(error);
                                    }
                                    result.Add(new_diagnostics);
                                }
                            }
                            else
                            {
                                System.Text.RegularExpressions.Match command_error_match = CommandErrorRegex.Match(line);
                                if (command_error_match.Value != "")
                                {
                                    string new_error = "[" + profile.Name + "] " + command_error_match.Value;
                                    if (!result.Contains(new_error))
                                    {
                                        result.Add(new_error);
                                    }
                                }
                            }
                        }
                    }
                    ++finish_count;
                    if (progress_method != null)
                    {
                        progress_method(((float)finish_count / (float)max_progress) * 100.0f);
                    }
                }                
            }
        }

        /// <summary>
        /// ファイルノードの検索
        /// </summary>
        /// <param name="root"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        static private FileNode SearchFileNode(AnalyzeResultNode root, string file_name)
        {
            if (root is FileNode)
            {
                FileNode file_node = root as FileNode;
                if (file_node.FileName == file_name)
                {
                    return file_node;
                }
            }

            foreach (AnalyzeResultNode node in root.Children)
            {
                FileNode find = SearchFileNode(node, file_name);
                if (find != null)
                {
                    return find;
                }
            }

            return null;
        }

        /// <summary>
        /// ファイルノードの検索
        /// </summary>
        /// <param name="root"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        static private FolderNode SearchFolderNode(AnalyzeResultNode root, string directory_name)
        {
            if (directory_name[directory_name.Length - 1] == '/')
            {
                directory_name = directory_name.Remove(directory_name.Length - 1);
            }

            if (root is FolderNode)
            {
                FolderNode dolder_node = root as FolderNode;
                if (dolder_node.DirectoryName == directory_name)
                {
                    return dolder_node;
                }
            }

            foreach (AnalyzeResultNode node in root.Children)
            {
                FolderNode find = SearchFolderNode(node, directory_name);
                if (find != null)
                {
                    return find;
                }
            }

            return null;
        }

        /// <summary>
        /// フォルダノードの作成
        /// </summary>
        /// <param name="root"></param>
        /// <param name="directory_name"></param>
        /// <returns></returns>
        static private FolderNode MakeFolderNode(AnalyzeResultNode root, string directory_name)
        {
            string[] folders = directory_name.Split('/');
            string search_directory_name = "";
            AnalyzeResultNode last_serach_node = root;
            bool not_found = false;
            foreach (string folder_name in folders)
            {
                search_directory_name += folder_name;

                if (not_found == false)
                {
                    FolderNode serach_node = SearchFolderNode(root, search_directory_name);

                    if (serach_node == null)
                    {
                        not_found = true;
                        FolderNode new_folder = new FolderNode(search_directory_name);
                        last_serach_node.Children.Add(new_folder);
                        last_serach_node = new_folder;
                    }
                    else
                    {
                        last_serach_node = serach_node;
                    }
                }
                else
                {
                    FolderNode new_folder = new FolderNode(search_directory_name);
                    last_serach_node.Children.Add(new_folder);
                    last_serach_node = new_folder;
                }

                search_directory_name += '/';
            }

            // 必ずフォルダノードのはず.
            return last_serach_node as FolderNode;
        }
    }
}
