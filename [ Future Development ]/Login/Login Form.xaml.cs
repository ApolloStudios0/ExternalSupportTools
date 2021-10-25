using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VirtualKeyboard.Wpf;

namespace ExternalSupportTools.__Future_Development__.Login
{
    public partial class Login_Form : Window
    {
        // Core Resources
        #region Core Logic

        // Popup Keyboard Method
        public async void OpenPopupKeyboard() { var value = await VKeyboard.OpenAsync(); }

        public Login_Form()
        {
            // Load Form
            InitializeComponent();

            // Focus On Login
            PasswordBox.Focus();
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Drag Window
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CloseButton(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        // Login Resources [ Bypass / PasswordBox / Enter Key ]
        #region Resources

        private async void CheckLoginDetails()
        {
            if (PasswordBox.Password.ToString() == "31415926535")
            {
                // Check 3141 Password
                _Main_Menu.Main_Menu main_Menu = new();
                main_Menu.Show();
                this.Close();
            }
            else
            {
                // Make Login Button Turn Red If Password Wrong [ 1 Second ]
                LoginButton.Background = Brushes.Red; await Task.Delay(1000); LoginButton.Background = (Brush)(new BrushConverter().ConvertFrom("#1666e1"));
            }
        }
        private void BypassLoginForm(object sender, MouseButtonEventArgs e)
        {
            // Bypass Login Form
            _Main_Menu.Main_Menu main_Menu = new();
            main_Menu.Show();
            this.Close();
        }
        private void Password_Enter(object sender, KeyEventArgs e)
        {
            // Password Box Enter Key
            if (e.Key == Key.Return) { CheckLoginDetails(); }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            CheckLoginDetails();
        }
        #endregion
    }
}
