using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
using System.Configuration;
using System.IO;

namespace mSlideViewer
{
    /// <summary>
    /// Option.xaml 的交互逻辑
    /// </summary>
    public partial class OptionDialog : Window
    {     
        public OptionDialog()
        {
            InitializeComponent();

            txTalkApplication.Text =  SettingsManager.ApplicationSettings.mSlideTalkAppPath;
            txTalkServerIP.Text = SettingsManager.ApplicationSettings.mSlideTalkServerIP;

            cbFavoriteItemsLoadOnStartup.IsChecked = SettingsManager.ApplicationSettings.FavoriteItemsLoadOnStartup;
            cbFavoriteItemsSaveOnExit.IsChecked = SettingsManager.ApplicationSettings.FavoriteItemsSaveOnExit;

            cbSaveSnapShotWhenExit.IsChecked = SettingsManager.ApplicationSettings.SaveSnapShotOnExit;
            cbCTRLforScale.IsChecked = SettingsManager.ApplicationSettings.CTRLforScale;
        }

        private void tbTalkApplicationPath_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = @"C:\Program files\";
            
            // Set filter for file extension and default file extension      
            dlg.Filter = "mSlide Virtual Slide Tiff (*.exe)|*.exe";
            dlg.FilterIndex = 1;

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                string fileName = System.IO.Path.GetFileName(dlg.FileName);
                if (fileName == "mSlideTalk.exe")
                {
                    SettingsManager.ApplicationSettings.mSlideTalkAppPath = dlg.FileName;
                    txTalkApplication.Text = dlg.FileName;
                }              
            }
        }

        private void tbServerConnectionTest_Click(object sender, RoutedEventArgs e)
        {
            string serverIP = txTalkServerIP.Text;
            SettingsManager.ApplicationSettings.mSlideTalkServerIP = serverIP;
            
            //  ConfigurationManager.AppSettings["ServerIP"], int.Parse(ConfigurationManager.AppSettings["ServerPort"])

            string fullPath = System.IO.Path.GetDirectoryName(txTalkApplication.Text);
            if (Directory.Exists(fullPath))
            {
                string configFileName = fullPath + @"\mSlideTalk.exe.config";
                System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(txTalkApplication.Text);
                UpdateAppConfig(config, "ServerIP", txTalkServerIP.Text);
            }
        }


        ///<summary>  
        ///在*.exe.config文件中appSettings配置节增加一对键值对  
        ///</summary>  
        ///<param name="newKey"></param>  
        ///<param name="newValue"></param>  
        public static void UpdateAppConfig(Configuration config, string newKey, string newValue)
        {          
            bool exist = false;
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == newKey)
                {
                    exist = true;
                }
            }

            if (exist)
            {
                config.AppSettings.Settings.Remove(newKey);
            }
            config.AppSettings.Settings.Add(newKey, newValue);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            // 参数保存操作
            if (cbCTRLforScale.IsChecked == true)
                SettingsManager.ApplicationSettings.CTRLforScale = true;            
            else
                SettingsManager.ApplicationSettings.CTRLforScale = false;

            if (cbFavoriteItemsSaveOnExit.IsChecked == true)
                SettingsManager.ApplicationSettings.FavoriteItemsSaveOnExit = true;            
            else
                SettingsManager.ApplicationSettings.FavoriteItemsSaveOnExit = false;

            if (cbFavoriteItemsLoadOnStartup.IsChecked == true)
                SettingsManager.ApplicationSettings.FavoriteItemsLoadOnStartup = true;
            else
                SettingsManager.ApplicationSettings.FavoriteItemsSaveOnExit = false;

            // 关闭
            this.Close();
        }

      
    }
}
