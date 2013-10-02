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

namespace ClangAnalyze
{
    /// <summary>
    /// UserControl1.xaml の相互作用ロジック
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        public EditableTextBlock()
        {
            InitializeComponent();
        }
 
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
 
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), 
              typeof(EditableTextBlock), new UIPropertyMetadata());

        /// <summary>
        /// エディット開始.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBlock_MouseRightButtonUp(object sender
                                       , MouseButtonEventArgs e)
        {
            EditStart();
        }
        private void EditStart()
        {
            textBlock.Visibility = System.Windows.Visibility.Collapsed;
            editBox.Text = textBlock.Text;
            editBox.Visibility = Visibility.Visible;
            editBox.Height = textBlock.Height;
            editBox.Width = textBlock.Width;
            editBox.Focus();
        }
 
        /// <summary>
        /// エディット終了.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EndEdit();
        }
        private void editBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EndEdit();
            }
        }
        private void EndEdit()
        {
            textBlock.Text = editBox.Text;
            textBlock.Visibility = System.Windows.Visibility.Visible;
            editBox.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
