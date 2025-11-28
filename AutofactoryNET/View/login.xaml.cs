using AutofactoryNET;
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

namespace AppAutoFactory.Pages
{
    /// <summary>
    /// Lógica de interacción para login.xaml
    /// </summary>
    public partial class login : Window
    {
        Dictionary<string, string> users = new Dictionary<string, string>()
        {
            {"email@autofactory.org", "1234"},
            {"mila", "milamila"},
            {"sara", "mila1234"}
        };
        string input_user;
        string input_password;

        public login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            input_user = txtUser.Text;
            input_password = txtPassword.Text;

            if (users.ContainsKey(input_user) && users[input_user] == input_password)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
