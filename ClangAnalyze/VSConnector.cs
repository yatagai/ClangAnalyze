using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

namespace ClangAnalyze
{
    /// <summary>
    /// VisualSduioと連携用.
    /// </summary>
    public class VSConnector
    {
        static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
        [DllImport("ole32.dll")]
        static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        /// <summary>
        /// 現在開いているDTEのリストを取得.
        /// </summary>
        /// <returns></returns>
        static private List<EnvDTE.DTE> GetDTEList()
        {
            List<EnvDTE.DTE> dte_list = new List<EnvDTE.DTE>();

            IBindCtx bind_dtx = null;
            CreateBindCtx(0, out bind_dtx);

            IRunningObjectTable rot = null;
            bind_dtx.GetRunningObjectTable(out rot);

            IEnumMoniker enum_moniker = null;
            rot.EnumRunning(out enum_moniker);

            enum_moniker.Reset();

            for ( ; ; )
            {
                IMoniker[] monikers = { null };

                IntPtr fetched = IntPtr.Zero;
                if (enum_moniker.Next(1, monikers, fetched) != 0)
                {
                    break;
                }

                string display_name;
                monikers[0].GetDisplayName(bind_dtx, null, out display_name);

                if (display_name.StartsWith("!VisualStudio.DTE"))
                {
                    object obj;
                    EnvDTE.DTE dte = null;

                    rot.GetObject(monikers[0], out obj);
                    dte = obj as EnvDTE.DTE;

                    if (dte != null)
                    {
                        dte_list.Add(dte);
                    }
                }

                Marshal.ReleaseComObject(monikers[0]);
            }
            Marshal.ReleaseComObject(enum_moniker);
            Marshal.ReleaseComObject(rot);
            Marshal.ReleaseComObject(bind_dtx);

            return dte_list;
        }

        /// <summary>
        /// GetDTEList()で取得したDTEリストの解放.
        /// </summary>
        /// <param name="dte_list"></param>
        static private void ReleaDTEList(List<EnvDTE.DTE> dte_list)
        {
            foreach (EnvDTE.DTE dte in dte_list)
            {
                Marshal.ReleaseComObject(dte);
            }
            dte_list.Clear();
        }

        /// <summary>
        /// 現在開いているソリューション名の取得.
        /// </summary>
        /// <returns></returns>
        static public List<string> GetOpenSolution()
        {
            List<string> solusion_names = new List<string>();

            List<EnvDTE.DTE> dte_list = GetDTEList();

            foreach (EnvDTE.DTE dte in dte_list)
            {
                string solusion_name = dte.Solution.FullName.Replace("\\", "/");
                solusion_name = solusion_name.Replace("\\\\", "/");
                solusion_names.Add(solusion_name);
            }

            // unmanagedなので解放が必要.
            ReleaDTEList(dte_list);

            return solusion_names;
        }

        /// <summary>
        /// 指定したソリューションから指定したファイルを開く.
        /// </summary>
        /// <param name="solution_name"></param>
        /// <param name="source_file_name"></param>
        /// <param name="line_no"></param>
        /// <returns></returns>
        static public bool OpenSource(string solution_name, string source_file_name, int line_no)
        {
            List<EnvDTE.DTE> dte_list = GetDTEList();

            bool ret = false;
            try
            {
                EnvDTE.DTE target_dte = null;
                foreach (EnvDTE.DTE dte in dte_list)
                {
                    string name = dte.Solution.FullName.Replace("\\", "/");
                    name = name.Replace("\\\\", "/");
                    if (solution_name == name)
                    {
                        target_dte = dte;
                        break;
                    }
                }

                ret = target_dte != null;

                if (target_dte != null)
                {
                    target_dte.ExecuteCommand("of", "\"" + source_file_name + "\"");
                    target_dte.ExecuteCommand("GotoLn", line_no.ToString());
                    target_dte.MainWindow.Activate();
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                ret = false;
            }

            // unmanagedなので解放が必要.
            ReleaDTEList(dte_list);

            return ret;
        }

        /// <summary>
        /// 指定したソリューションの出力ウインドウへ文字列を表示.
        /// </summary>
        /// <param name="solution_name"></param>
        /// <param name="show_string"></param>
        /// <returns></returns>
        static public bool ShowStringOutPut(string solution_name, string show_string)
        {
            List<EnvDTE.DTE> dte_list = GetDTEList();

            bool ret = false;
            try
            {
                EnvDTE.DTE target_dte = null;
                foreach (EnvDTE.DTE dte in dte_list)
                {
                    string name = dte.Solution.FullName.Replace("\\", "/");
                    name = name.Replace("\\\\", "/");
                    if (solution_name == name)
                    {
                        target_dte = dte;
                        break;
                    }
                }

                ret = target_dte != null;
                if (target_dte != null)
                {
                    string vsWindowKindOutput = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";
                    vsWindowKindOutput = vsWindowKindOutput.ToLower();
                    EnvDTE.Window window = target_dte.Windows.Item(vsWindowKindOutput);
                    EnvDTE.OutputWindow output_window = window.Object as EnvDTE.OutputWindow;
                    EnvDTE.OutputWindowPane analyze_pane = null;
                    foreach (EnvDTE.OutputWindowPane pane in output_window.OutputWindowPanes)
                    {
                        if (pane.Name == "ClangAnalyze")
                        {
                            analyze_pane = pane;
                            break;
                        }
                    }
                    if (analyze_pane == null)
                    {
                        analyze_pane = output_window.OutputWindowPanes.Add("ClangAnalyze");
                    }
                    analyze_pane.Clear();
                    analyze_pane.OutputString(show_string);
                    target_dte.MainWindow.Activate();
                    window.Activate();
                    analyze_pane.Activate();
                }
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                ret = false;
            }

            // unmanagedなので解放が必要.
            ReleaDTEList(dte_list);

            return ret;
        }
    }
}
