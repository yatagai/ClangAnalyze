/**
 *  MainWindowViewModel.cs
 *  @brif メインウインドウのビューモデルクラス.
 **/

using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Collections.Generic;

namespace ClangAnalyze
{
    /// <summary>
    /// 汎用コマンドクラス.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        /// <param name="execute_handler"></param>
        /// <param name="can_execute_handler"></param>
        public DelegateCommand(ExecuteHandler execute_handler, CanExecuteHandler can_execute_handler)
        {
            OnExecute = execute_handler;
            OnCanExecute = can_execute_handler;
        }

        public delegate void ExecuteHandler(object parameter);
        private event ExecuteHandler OnExecute;
        public delegate bool CanExecuteHandler(object parameter);
        private event CanExecuteHandler OnCanExecute;

        /// <summary>
        /// コマンド実行
        /// </summary>
        public void Execute(object parameter)
        {
            if (OnExecute != null)
            {
                OnExecute(parameter);
            }
        }

        /// <summary>
        /// コマンド実行可能か.
        /// </summary>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            if (OnCanExecute != null)
            {
                return OnCanExecute(parameter);
            }
            return true;
        }

        public event System.EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    /// <summary>
    /// メインウインドウのビューモデルクラス
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public MainWindowViewModel()
        {
            AddProfileCommand = new DelegateCommand(AddProfile, CanAddProfile);
            DeleteProfileCommand = new DelegateCommand(DeleteProfile, CanDeleteProfile);
            SaveCommand = new DelegateCommand(Save, CanSave);
            LoadCommand = new DelegateCommand(Load, CanLoad);
            AnalyzeCommand = new DelegateCommand(Analyze, CanAnalyze);
            m_setting_file_name = "UNTITLED";

            AnalyzeResultNode root = new AnalyzeResultNode();
            root.Text = "ROOT";
            m_result_tree = new ObservableCollection<AnalyzeResultNode>();
            m_result_tree.Add(root);
            TreeViewSelectChengeCommand = new DelegateCommand(OnTreeViewSelectChange, null);


            VSConnector.GetOpenSolution();
        }

        /// <summary>
        /// プロパティ変更.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        #region profiles
        /// <summary>
        /// プロファイル.
        /// </summary>
        public ObservableCollection<Profile> Profiles
        {
            get
            {
                return m_analyze_setting.Profiles;
            }
            set
            {
                m_analyze_setting.Profiles = value;
                OnPropertyChanged("Profiles");
            }
        }

        /// <summary>
        /// 選択中のプロファイルインデックス.
        /// </summary>
        public int SelectedProfileIndex
        {
            get
            {
                return m_selected_profile_index;
            }
            set
            {
                m_selected_profile_index = value;
                OnPropertyChanged("SelectedProfileIndex");
                OnPropertyChanged("OptionEnable");
                OnPropertyChanged("SelectedProfileOptions");
            }
        }
        private int m_selected_profile_index = -1;

        /// <summary>
        /// Profile追加コマンド.
        /// </summary>
        public ICommand AddProfileCommand
        {
            get;
            private set;
        }
        private void AddProfile(object parameter)
        {
            Profile new_profile = new Profile();
            new_profile.Name = "NewProfile";
            m_analyze_setting.Profiles.Add(new_profile);
        }
        private bool CanAddProfile(object parameter)
        {
            return true;
        }

        /// <summary>
        /// プロファイル削除コマンド.
        /// </summary>
        public ICommand DeleteProfileCommand
        {
            get;
            private set;
        }
        private void DeleteProfile(object parameter)
        {
            Profiles.RemoveAt(m_selected_profile_index);
        }
        private bool CanDeleteProfile(object parameter)
        {
            return m_selected_profile_index >= 0;
        }
        #endregion

        #region options
        /// <summary>
        /// オプション.
        /// </summary>
        public string SelectedProfileOptions
        {
            get
            {
                string ret = "";
                if (Profiles.Count > m_selected_profile_index &&
                    m_selected_profile_index >= 0)
                {  
                    foreach (string option in m_analyze_setting.Profiles[m_selected_profile_index].Options)
                    {
                        ret += option + "\r\n";
                    }
                }
                return ret;
            }
            set
            {
                if (Profiles.Count > m_selected_profile_index &&
                    m_selected_profile_index >= 0)
                {
                    string replace_value = value.Replace("\r\n", "\n");
                    string[] options = replace_value.Split('\n');
                    m_analyze_setting.Profiles[m_selected_profile_index].Options.Clear();
                    foreach (string option in options)
                    {
                        if (option != "")
                        {
                            m_analyze_setting.Profiles[m_selected_profile_index].Options.Add(option);
                        }
                    }
                }
                OnPropertyChanged("SelectedProfileOptions");
            }
        }

        /// <summary>
        /// オプションコントロールの有効無効.
        /// </summary>
        public bool OptionEnable
        {
            get
            {
                return m_selected_profile_index >= 0;
            }
        }
        #endregion

        #region setting
        public string AnalyzeDirectory
        {
            get
            {
                return m_analyze_setting.AnalyzeDirectory;
            }
            set
            {
                m_analyze_setting.AnalyzeDirectory = value;
                OnPropertyChanged("AnalyzeDirectory");
            }
        }
        /// <summary>
        /// 設定ファイル名.
        /// </summary>
        public string SettingFileName
        {
            get
            {
                return m_setting_file_name;
            }
            set
            {
                m_setting_file_name = value;
                OnPropertyChanged("SettingFileName");
            }
        }
        private string m_setting_file_name;

        /// <summary>
        /// SAVEコマンド.
        /// </summary>
        public ICommand SaveCommand
        {
            get;
            private set;
        }
        private void Save(object parameter)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.FileName = "";
            save.Filter = "jsonファイル(*.json)|*.json";
            save.DefaultExt = "*.json";
            if (save.ShowDialog() == true)
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.Formatting = Formatting.Indented;

                using (StreamWriter sw = new StreamWriter(save.FileName))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, m_analyze_setting);
                    }
                }
                SettingFileName = save.FileName;
            }
        }
        private bool CanSave(object parameter)
        {
            return true;
        }
        public ICommand LoadCommand
        {
            get;
            private set;
        }
        private void Load(object parameter)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.FileName = "";
            open.Filter = "jsonファイル(*.json)|*.json";
            open.DefaultExt = "*.json";
            if (open.ShowDialog() == true)
            {
                using (StreamReader sr = new StreamReader(open.FileName))
                {
                    string text = sr.ReadToEnd();
                    m_analyze_setting = JsonConvert.DeserializeObject<AnalyzeSetting>(text);
                }
                SettingFileName = open.FileName;

                OnPropertyChanged("AnalyzeDirectory");
                OnPropertyChanged("Profiles");
                SelectedProfileIndex = -1;
            }
        }
        private bool CanLoad(object parameter)
        {
            return true;
        }

        AnalyzeSetting m_analyze_setting = new AnalyzeSetting();
        #endregion

        #region analyzer
        /// <summary>
        /// Analyzeコマンド.
        /// </summary>
        public ICommand AnalyzeCommand
        {
            get;
            private set;
        }
        private void Analyze(object parameter)
        {
            if (!System.IO.Directory.Exists(m_analyze_setting.AnalyzeDirectory))
            {
                ResultText = "error: Not Found Directory " + m_analyze_setting.AnalyzeDirectory;
            }
            else
            {
                ResultText = "ANALYZING-0%";
                m_result_tree[0].Children = new List<AnalyzeResultNode>();
                IsAnalyzing = true;
                AnalyzeProgressCount = 0;

                // UIが固まるので別スレで実行.
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(AnalyzeWorker_DoWork);
                worker.ProgressChanged += new ProgressChangedEventHandler(AnalyzeWorker_ProgressChanged);
                worker.WorkerReportsProgress = true;
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(AnalyzeWorker_RunWorkerCompleted);
                worker.RunWorkerAsync();
               
            }
        }

        void AnalyzeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Analyzer.Analyze(m_analyze_setting, m_result_tree[0], delegate (float progress_count)
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                worker.ReportProgress((int)progress_count);
            });

            // 100%を見せつけたいので少し止める.
            System.Threading.Thread.Sleep(300);
        }
        void AnalyzeWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            AnalyzeProgressCount = e.ProgressPercentage;
            if (AnalyzeProgressCount < 100)
            {
                ResultText = "ANALYZING-" + AnalyzeProgressCount.ToString() + "%";
            }
            else
            {
                ResultText = "FINISH ANALYZE-" + AnalyzeProgressCount.ToString() + "%";
            }
        }
        void AnalyzeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsAnalyzing = false;

            if (e.Cancelled)
            {
                ResultText = "CANCEL ANALYZE";
            }
            else
            {
                OnTreeViewSelectChange(m_result_tree[0]);
            }

            // ANALYZEボタンがアクティブにならないことがあるので.
            CommandManager.InvalidateRequerySuggested();
        }
        
        private bool CanAnalyze(object parameter)
        {
            return !IsAnalyzing;
        }
        private bool IsAnalyzing
        {
            get
            {
                return m_is_analyzing;
            }
            set
            {
                m_is_analyzing = value;
                OnPropertyChanged("IsAnalyzing");
                OnPropertyChanged("NoAnalyzing");
                OnPropertyChanged("ResultTree");

            }
        }
        public bool NoAnalyzing
        {
            get
            {
                return !m_is_analyzing;
            }
        }
        private bool m_is_analyzing;
        public int AnalyzeProgressCount
        {
            get
            {
                return m_analyze_progress_count;
            }
            set
            {
                m_analyze_progress_count = value;
                OnPropertyChanged("AnalyzeProgressCount");
            }
        }
        private int m_analyze_progress_count;
        #endregion

        #region result
        /// <summary>
        /// リザルトテキスト.
        /// </summary>
        public string ResultText
        {
            get
            {
                return m_result_text;
            }
            set
            {
                m_result_text = value;
                OnPropertyChanged("ResultText");
            }
        }
        private string m_result_text;
        /// <summary>
        /// リザルトツリー.
        /// </summary>
        public ObservableCollection<AnalyzeResultNode> ResultTree
        {
            get
            {
                return IsAnalyzing ? null : m_result_tree;
            }
            private set
            {
                m_result_tree = value;
            }
        }
        private ObservableCollection<AnalyzeResultNode> m_result_tree;

        /// <summary>
        /// ツリービュークリックコマンド.
        /// </summary>
        public ICommand TreeViewSelectChengeCommand
        {
            get;
            private set;
        }
        private void OnTreeViewSelectChange(object parameter)
        {
            if (parameter == null)
            {
                return;
            }
            AnalyzeResultNode node = parameter as AnalyzeResultNode;
            if (node == null)
            {
                return;
            }
            ResultText = GetResultText(node) + "\n";
        }

        /// <summary>
        /// リザルトテキストの取得.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private string GetResultText(AnalyzeResultNode node)
        {
            string result = "";

            if (node is FileNode)
            {
                foreach (AnalyzeResultNode chid_node in node.Children)
                {
                    if (result != "")
                    {
                        result += "\n";
                    }
                    result += chid_node.Text;
                }
            }
            else
            {
                foreach (AnalyzeResultNode chid_node in node.Children)
                {
                    if (result != "")
                    {
                        result += "\n";
                    }
                    result += GetResultText(chid_node);
                }
            }

            return result;
        }
        #endregion
    }
}
