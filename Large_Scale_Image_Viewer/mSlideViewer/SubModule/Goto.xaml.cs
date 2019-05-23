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
using System.Windows.Shapes;

namespace mSlideViewer
{
    /// <summary>
    /// Goto.xaml 的交互逻辑
    /// </summary>
    public partial class GotoDialog : Window
    {
        // Center Location
        public int XPosition;
        public int YPosition;

        public int XPositionMax = 80000;
        public int YPositionMax = 80000;

        public string Mag;
        
        public GotoDialog()
        {
            InitializeComponent();
            
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btGoto_Click(object sender, RoutedEventArgs e)
        {
            if (tbXPosition.Text != "" && tbYPosition.Text != "")
            {
                XPosition = Convert.ToInt32(tbXPosition.Text);
                YPosition = Convert.ToInt32(tbYPosition.Text);
                Mag = cbMag.SelectedItem as string;

                this.DialogResult = true;
            } 
            else
            {
                this.DialogResult = false;
            
            }           
         
        }

        private void IntergeTextBoxKeyPress(object sender, KeyEventArgs e)
        {
            var txt = sender as TextBox;
            
            //屏蔽非法按键
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                e.Handled = false;
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                e.Handled = false;
            }
            else if (e.Key == Key.Enter)
            {
                e.Handled = false;
            }
            else if (e.Key == Key.Delete || e.Key == Key.Back || e.Key == Key.Tab)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void IntergeTextBoxTextChange(object sender, TextChangedEventArgs e, int min, int max)
        {
            TextBox textBox = sender as TextBox;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textBox.Text, out num))
                {
                    textBox.Text = textBox.Text.Remove(offset, change[0].AddedLength);
                    textBox.Select(offset, 0);
                    
                }
                else
                {
                    if (num >= max) textBox.Text = max.ToString();
                    if (num <= min) textBox.Text = min.ToString();

                }
            }
        }

        private void tbXPosition_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            IntergeTextBoxKeyPress(sender, e);
        }

        private void tbYPosition_TextChanged(object sender, TextChangedEventArgs e)
        {
            IntergeTextBoxTextChange(sender, e, 0, YPositionMax);
        }

        private void tbYPosition_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            IntergeTextBoxKeyPress(sender, e);
        }   

        private void tbXPosition_TextChanged(object sender, TextChangedEventArgs e)
        {
            IntergeTextBoxTextChange(sender, e, 0, XPositionMax);
        }

        private void txPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            IntergeTextBoxKeyPress(sender, e);
        }

        private void txPage_TextChanged(object sender, TextChangedEventArgs e)
        {
            IntergeTextBoxTextChange(sender, e, 0, 3);
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mainWin = (MainWindow)this.Owner;

            List<string> data = new List<string>();
            if (mainWin.magnifier == Magnifier.X20)
            {               
                data.Add("20.0X");
                data.Add("10.0X");
                data.Add("5.0X");
                data.Add("3.75X");
                data.Add("2.5X");
                data.Add("1.25X");
            }

            if (mainWin.magnifier == Magnifier.X40)
            {    
                data.Add("40.0X");
                data.Add("20.0X");
                data.Add("10.0X");
                data.Add("7.5X");
                data.Add("5.0X");
                data.Add("2.5X");
            }

            // Get the ComboBox reference.
            var comboBox = sender as ComboBox;

            // Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the first item selected.
            comboBox.SelectedIndex = 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            Mag = comboBox.SelectedItem as string;
        }
    }
}
