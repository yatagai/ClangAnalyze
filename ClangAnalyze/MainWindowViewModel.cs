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
            m_setting_file_name = "UNTITLED";
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
                OnPropertyChanged("SelectedProfileIs32Bit");
                OnPropertyChanged("SelectedProfileIs64Bit");
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
        /// 32bitか.
        /// </summary>
        public bool SelectedProfileIs32Bit
        {
            get
            {
                if (Profiles.Count > m_selected_profile_index &&
                    m_selected_profile_index >= 0)
                {
                    return m_analyze_setting.Profiles[m_selected_profile_index].Bit == 32;
                }
                return false;
            }
            set
            {
                if (Profiles.Count > m_selected_profile_index &&
                    m_selected_profile_index >= 0)
                {
                    if (value == true)
                    {
                        m_analyze_setting.Profiles[m_selected_profile_index].Bit = 32;
                    }
                    else
                    {
                        m_analyze_setting.Profiles[m_selected_profile_index].Bit = 64;
                    }
                }
                OnPropertyChanged("SelectedProfileIs32Bit");
                OnPropertyChanged("SelectedProfileIs64Bit");
            }
        }

        /// <summary>
        /// 64bitか.
        /// </summary>
        public bool SelectedProfileIs64Bit
        {
            get
            {
                if (Profiles.Count > m_selected_profile_index &&
                    m_selected_profile_index >= 0)
                {
                    return m_analyze_setting.Profiles[m_selected_profile_index].Bit == 64;
                }
                return false;
            }
            set
            {
                if (Profiles.Count > m_selected_profile_index &&
                    m_selected_profile_index >= 0)
                {
                    if (value == true)
                    {
                        m_analyze_setting.Profiles[m_selected_profile_index].Bit = 64;
                    }
                    else
                    {
                        m_analyze_setting.Profiles[m_selected_profile_index].Bit = 32;
                    }
                }
                OnPropertyChanged("SelectedProfileIs32Bit");
                OnPropertyChanged("SelectedProfileIs64Bit");
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
    }
}
