using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace mSlideViewer
{
    /// <summary>
    /// AnnotationDiscription.xaml 的交互逻辑
    /// </summary>
    public partial class AnnotationDiscription : Window
    {
        public string ReportDiscription;
        public string Reporter ;
        public string ReportDate;
        
        public AnnotationDiscription()
        {
            InitializeComponent();

            ReportDiscription = "";
            Reporter = "";
            ReportDate = "";
            string[] lines = File.ReadAllLines("favorite.txt");
            foreach (string fi in lines)
            {
                favorite.Items.Add(fi);
            }
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {  
            this.DialogResult = true;
        }
        private void ListBoxItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            txReport.AppendText(favorite.SelectedItem.ToString()+"; ");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            txReport.AppendText(favorite.SelectedItem.ToString() + "; ");
        }
    }
}
