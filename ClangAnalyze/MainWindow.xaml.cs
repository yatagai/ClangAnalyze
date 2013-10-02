using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirstFloor.ModernUI.Windows.Controls;

namespace ClangAnalyze
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Style = (Style)App.Current.Resources["EmptyWindow"];
            this.DataContext = new MainWindowViewModel();
            window_title.DataContext = this;
        }

        /// <summary>
        /// ソリューションリストドロップダウンが開いたとき.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlnList_DropDownOpened(object sender, EventArgs e)
        {
            UpdateSlnList();
        }

        /// <summary>
        /// ソリューションリストドロップダウンが読み込まれたとき.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlnList_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSlnList();
        }

        /// <summary>
        /// ソリューションリスト更新.
        /// </summary>
        private void UpdateSlnList()
        {
            sln_list.Items.Clear();
            List<string> sln_names = VSConnector.GetOpenSolution();
            foreach (string sln_name in sln_names)
            {
                sln_list.Items.Add(sln_name);
            }
            if (sln_list.Items.Count > 0)
            {
                sln_list.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// VS OUTPUTがクリックされた.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VSConnector.ShowStringOutPut(sln_list.Text, result_text.Text);
        }

        /// <summary>
        /// リザルトテキストがクリックされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultText_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex(
                    @"([A-Z]:[\\/].*?)\(([0-9]*).*",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            System.Text.RegularExpressions.Match match = regex.Match(result_text.SelectedText);
            if (match.Success && match.Groups.Count >= 3)
            {
                string src_name = match.Groups[1].Value;
                int line_no = int.Parse(match.Groups[2].Value);
                if (System.IO.File.Exists(src_name))
                {
                    VSConnector.OpenSource(sln_list.Text, src_name, line_no);
                }
            }
        }
    }

    /// <summary>
    /// ツリービュークリックビヘイビア.
    /// </summary>
    public class TreeViewBehaviors
    {
        public static ICommand GetOnSelectedItemChanged(DependencyObject d)
        {
            return (ICommand)d.GetValue(OnSelectedItemChangedProperty);
        }

        public static void SetOnSelectedItemChanged(DependencyObject d, ICommand value)
        {
            d.SetValue(OnSelectedItemChangedProperty, value);
        }

        public static readonly DependencyProperty OnSelectedItemChangedProperty =
            DependencyProperty.RegisterAttached("OnSelectedItemChanged", typeof(ICommand), typeof(TreeViewBehaviors), new UIPropertyMetadata(null, OnSelectedItemChangedPropertyChanged));

        static void OnSelectedItemChangedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var tree_view = d as TreeView;
            if (tree_view == null)
                return;

            if (args.NewValue is ICommand)
                tree_view.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(OnTreeViewSelectedItemChanged);
            else
                tree_view.SelectedItemChanged -= new RoutedPropertyChangedEventHandler<object>(OnTreeViewSelectedItemChanged);
        }

        /// <summary>
        /// コマンド実行.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree_view = e.OriginalSource as TreeView;
            if (tree_view == null)
                return;

            ICommand command = GetOnSelectedItemChanged(tree_view);
            if (command == null)
                return;

            if (tree_view.SelectedItem == null)
            {
                return;
            }

            command.Execute(tree_view.SelectedItem);            
        }
    }
}
