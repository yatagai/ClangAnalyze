using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace ClangAnalyze
{
    /// <summary>
    /// リザルトノード.
    /// @note 親の参照を持つと循環参照によるメモリリークが起こるので禁止.
    /// </summary>
    public class AnalyzeResultNode
    {
        public AnalyzeResultNode()
        {
            Children = new List<AnalyzeResultNode>();
        }
        public string Text
        {
            get
            {
                return m_text;
            }
            set
            {
                m_text = value;
            }
        }
        private string m_text;

        public List<AnalyzeResultNode> Children
        {
            get;
            set;
        }
    }

    /// <summary>
    /// フォルダノード.
    /// @note 親の参照を持つと循環参照によるメモリリークが起こるので禁止.
    /// </summary>
    public class FolderNode : AnalyzeResultNode
    {
        public FolderNode(string directory_name)
        {
            DirectoryName = directory_name;

            int delete_end = directory_name.LastIndexOf('/');
            if (delete_end >= 0)
            {
                directory_name = directory_name.Remove(0, delete_end + 1);
            }
            Text = directory_name;
        }

        public string DirectoryName
        {
            get;
            set;
        }
    }

    /// <summary>
    /// ファイルノード.
    /// @note 親の参照を持つと循環参照によるメモリリークが起こるので禁止.
    /// </summary>
    public class FileNode : AnalyzeResultNode
    {
        public FileNode(string file_name)
        {
            file_name = file_name.Replace("\\", "/");
            file_name = file_name.Replace("\\\\", "/");
            Text = System.IO.Path.GetFileName(file_name);
            FileName = file_name;
            
            // directory name.
            DirectoryName = System.IO.Path.GetDirectoryName(file_name);
            DirectoryName = DirectoryName.Replace("\\", "/");
            DirectoryName = DirectoryName.Replace("\\\\", "/");

            if (DirectoryName[DirectoryName.Length - 1] == '/')
            {
                DirectoryName = DirectoryName.Remove(DirectoryName.Length - 1);
            }
        }

        public string FileName
        {
            get;
            set;
        }

        public string DirectoryName
        {
            get;
            set;
        }

        public Brush TextColor
        {
            get
            {
                foreach (AnalyzeResultNode child in Children)
                {
                    if (child is ErrorNode)
                    {
                        return Brushes.Red;
                    }
                }
                return Brushes.Yellow;
            }
        }
    }

    /// <summary>
    /// 診断結果ノード.
    /// @note 親の参照を持つと循環参照によるメモリリークが起こるので禁止.
    /// </summary>
    public class DiagnosticNode : AnalyzeResultNode
    {
        public DiagnosticNode(string file_name, int line_no, int column_no, string error_text)
        {
            FileName = file_name;
            LineNo = line_no;
            ColumnNo = column_no;
            Text = error_text;
        }

        public string FileName
        {
            get;
            private set;
        }

        public int LineNo
        {
            get;
            private set;
        }

        public int ColumnNo
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// エラーノード.
    /// @note 親の参照を持つと循環参照によるメモリリークが起こるので禁止.
    /// </summary>
    public class ErrorNode : DiagnosticNode
    {
        public ErrorNode(string file_name, int line_no, int column_no, string error_text)
            : base(file_name, line_no, column_no, error_text)
        {
        }
    }

    /// <summary>
    /// ワーニングノード.
    /// @note 親の参照を持つと循環参照によるメモリリークが起こるので禁止.
    /// </summary>
    public class WarningNode : DiagnosticNode
    {
        public WarningNode(string file_name, int line_no, int column_no, string warning_text)
            : base(file_name, line_no, column_no, warning_text)
        {
        }
    }
}
