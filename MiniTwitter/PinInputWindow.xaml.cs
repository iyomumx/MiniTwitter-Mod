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
using System.Windows.Shapes;

namespace MiniTwitter
{
    /// <summary>
    /// PinInputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PinInputWindow : Window
    {
        public readonly static DependencyProperty PinProperty = DependencyProperty.Register("PIN", typeof(string), typeof(PinInputWindow), new PropertyMetadata("PIN Here"));
        
        public string Pin
        {
            get { return (string)GetValue(PinProperty); }
            set { SetValue(PinProperty, value); }
        }

        public PinInputWindow()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public static string PinBox(out bool? dialogResult)
        {
            PinInputWindow window = new PinInputWindow();
            string pin = string.Empty;
            window.textBox1.SelectAll();
            dialogResult = window.ShowDialog();
            if (dialogResult == true)
                pin = window.Pin;
            return pin;
        }

        public static string PinBox()
        {
            bool? temp;

            return PinBox(out temp);
        }

        private void textBox1_MouseEnter(object sender, MouseEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            if (!IsVisible)
            {
                Show();
            }

            Activate();
            textBox1.Focus();
            textBox1.SelectAll();
        }
    }
}
