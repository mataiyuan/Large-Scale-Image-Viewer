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
using System.Text.RegularExpressions;

namespace Utilities
{
    /// <summary>
    /// Interaction logic for IPAddressControl.xaml
    /// </summary>
    public partial class IPAddressControl : UserControl
    {

        #region Constructor

        /// <summary>
        ///  Constructor for the control.
        /// </summary>
        public IPAddressControl()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(IPAddressControl_Loaded);
            // txtboxFirstPart.Text = "0";
        }

        #endregion

        #region Private Variables

        private bool focusMoved = false;

        #endregion

        #region Private Methods

        private static void TextboxTextCheck(object sender)
        {
            TextBox txtbox = (TextBox)sender;
            txtbox.Text = GetNumberFromString(txtbox.Text);
            if (!string.IsNullOrWhiteSpace(txtbox.Text))
            {
                if (Convert.ToInt32(txtbox.Text) > 255)
                {
                    txtbox.Text = "255";
                }
                else if (Convert.ToInt32(txtbox.Text) < 0)
                {
                    txtbox.Text = "0";
                }
            }

            txtbox.CaretIndex = txtbox.Text.Length;
        }

        private static string GetNumberFromString(string str)
        {
            StringBuilder numberBuilder = new StringBuilder();
            foreach (char c in str)
            {
                if (char.IsNumber(c))
                {
                    numberBuilder.Append(c);
                }
            }
            return numberBuilder.ToString();
        }

        #endregion

        #region Private Events

        void IPAddressControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtboxFirstPart.Focus();
        }

        private void txtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextboxTextCheck(sender);
            }
            catch (Exception ex)
            {
                throw new Exception("An Error occured in the control.", ex);
            }
        }

        private void txtboxFirstPart_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.OemPeriod)
                {
                    txtboxSecondPart.Focus();
                    focusMoved = true;
                }
                else
                {
                    focusMoved = false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An Error occured in the control.", ex);
            }

        }

        private void txtboxSecondPart_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.OemPeriod && !focusMoved)
                {
                    txtboxThridPart.Focus();
                    focusMoved = true;
                }
                else
                {
                    focusMoved = false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An Error occured in the control.", ex);
            }

        }

        private void txtboxThridPart_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.OemPeriod)
                {
                    txtboxFourthPart.Focus();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An Error occured in the control.", ex);
            }
        }

        #endregion

        #region Public Properties
        private string _text;
        /// <summary>
        /// Gets or Sets the text of the control.
        /// If input text is not of IP type type then throws and argument exception.
        /// </summary>
        public string Text
        {
            get
            {
                _text = txtboxFirstPart.Text + "." + txtboxSecondPart.Text + "." + txtboxThridPart.Text + "." + txtboxFourthPart.Text;
                return _text;
        
            }
            set
            {
                try
                {
                    string[] splitValues = value.Split('.');
                    txtboxFirstPart.Text = splitValues[0];
                    txtboxSecondPart.Text = splitValues[1];
                    txtboxThridPart.Text = splitValues[2];
                    txtboxFourthPart.Text = splitValues[3];
                    _text = value;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Input text is not of IP address type.", ex);
                }
            }

        }
        #endregion

        private void txtboxFirstPart_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox txtbox = (TextBox)sender;
                if (txtbox.Text.Length == 3)
                {
                    txtboxSecondPart.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void txtboxSecondPart_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox txtbox = (TextBox)sender;
                if (txtbox.Text.Length == 3)
                {
                    txtboxThridPart.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void txtboxThridPart_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                TextBox txtbox = (TextBox)sender;
                if (txtbox.Text.Length == 3)
                {
                    txtboxFourthPart.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}