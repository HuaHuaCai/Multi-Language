using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MultiLanguage;

namespace MultiLanguage_wpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        MultiLanguageMng languageMng;
        MultiLanguageMng LanguageMng
        {
            get
            {
                if (languageMng == null) languageMng = new MultiLanguageMng();
                return languageMng;
            }
        }


        public MainWindow()
        {
            InitializeComponent();
        }


        private void box1_PreviewDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            string fileName = System.IO.Path.GetDirectoryName(path);

            box1.Text = fileName;
        }

        private void box1_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            string path = string.Empty;
            if (box1.Text != string.Empty) path = box1.Text;
            else path = "../../../Documents/多语言配置信息.xlsx";

            LanguageMng.CreateBinaryData(path);
        }
    }
}
