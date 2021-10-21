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

namespace ExternalSupportTools
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            PasswordBox.Focus();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private async void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                #region Save Password, Possibly for future use?
                Properties.Settings.Default.Password = PasswordBox.Password.ToString();
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
                #endregion

                #region Check if password was correct
                if (Properties.Settings.Default.Password == "31415926535")
                {
                    MainMenu.MainForm mainForm = new();
                    mainForm.Show();
                    this.Close();
                }
                else
                {
                    #region Show Red Outlines For Wrong Password
                    BrushConverter bc = new BrushConverter();

                    // Change Border + Text To Red [ 2 seconds ]
                    WindowBorderMain.Stroke = Brushes.Red;
                    MainLabelText.Foreground = Brushes.Red;

                    // Seperation
                    await Task.Delay(2000);

                    // Change Border + Text To Default
                    WindowBorderMain.Stroke = (Brush)bc.ConvertFrom("#ff6e4f");
                    MainLabelText.Foreground = (Brush)bc.ConvertFrom("#ff6e4f");
                    #endregion
                }
                #endregion
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainMenu.MainForm mainForm = new();
            mainForm.Show();
            this.Close();
        }
    }
}
