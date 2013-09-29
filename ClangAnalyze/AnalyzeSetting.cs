/**
 *  AnalyzeSetting.cs
 *  @brif セッティングクラス.
 **/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ClangAnalyze
{
    /// <summary>
    /// プロファイルクラス.
    /// </summary>
    public class Profile
    {
        public Profile()
        {
            Options = new ObservableCollection<string>();
            Bit = 32;
        }

        public string Name
        {
            get;
            set;
        }

        public int Bit
        {
            get;
            set;
        }

        public ObservableCollection<string> Options
        {
            get;
            set;
        }
    }

    /// <summary>
    /// アナライズ設定クラス.
    /// </summary>
    public class AnalyzeSetting
    {
        public AnalyzeSetting()
        {
            Profiles = new ObservableCollection<Profile>();
        }

        /// <summary>
        /// 解析ディレクトリ.
        /// </summary>
        public string AnalyzeDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// プロファイル.
        /// </summary>
        public ObservableCollection<Profile> Profiles
        {
            get;
            set;
        }
    }
}
