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
    /// ResultTextBox.xaml の相互作用ロジック
    /// </summary>
    public partial class ResultTextBox : UserControl
    {
        public ResultTextBox()
        {
            InitializeComponent();
            base_text_box.Document.LineHeight = 0.5f;
        }

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
                BaseTextBox(value);
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(TextProperty, e.NewValue);
            ResultTextBox text_box = d as ResultTextBox;
            if (text_box != null)
            {
                text_box.BaseTextBox((string)e.NewValue);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
              typeof(ResultTextBox), new FrameworkPropertyMetadata(
                    new PropertyChangedCallback(OnTextChanged)
                ));

        /// <summary>
        /// ベーステキストボックスへ値を設定.
        /// </summary>
        /// <param name="value"></param>
        private void BaseTextBox(string value)
        {
            string text = value.Replace("\r\r\n", "\n");
            text = text.Replace("\r\n", "\n");
            string[] lines = text.Split('\n');

            base_text_box.Document.Blocks.Clear();
            foreach (string line in lines)
            {
                var paragraph = new Paragraph();
                
                if (line.Contains(" error: "))
                {
                    var span = new Span { Foreground = Brushes.Red };
                    span.Inlines.Add(line);
                    paragraph.Inlines.Add(span);
                }
                else if (line.Contains(" warning: "))
                {
                    var span = new Span { Foreground = Brushes.Yellow };
                    span.Inlines.Add(line);
                    paragraph.Inlines.Add(span);
                }
                else if (line == "no error")
                {
                    var span = new Span { Foreground = Brushes.Green };
                    span.Inlines.Add(line);
                    paragraph.Inlines.Add(span);
                }
                else
                {
                    paragraph.Inlines.Add(line);
                }
                // paragraph.Inlines.Add("\n");
                base_text_box.Document.Blocks.Add(paragraph);
            }
        }

        public string SelectedText
        {
            get;
            set;
        }
        private void base_text_box_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextPointer text_pointer = base_text_box.Selection.Start.GetLineStartPosition(0);
            string text = new TextRange(text_pointer.Paragraph.ContentStart, text_pointer.Paragraph.ContentEnd).Text;
            SelectedText = text;
        }

    }
}
