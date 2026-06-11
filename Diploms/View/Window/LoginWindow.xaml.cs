using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace SilaLesaWpfApp.View.Window
{
    public partial class LoginWindow : System.Windows.Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            LoadRoles();
        }

        private void LoadRoles()
        {
            cmbRole.ItemsSource = App.context.Roles;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                txtError.Text = "Введите логин и пароль";
                txtError.Visibility = Visibility.Visible;
                return;
            }

                // Проверка выбранной роли
                if (cmbRole.SelectedValue != null)
                {
                    string selectedRole = ((DataRowView)cmbRole.SelectedItem)["RoleName"].ToString();
                    if (selectedRole != App.currentUser.Roles.RoleName)
                    {
                        txtError.Text = "Выбранная роль не соответствует вашей учетной записи";
                        txtError.Visibility = Visibility.Visible;
                        return;
                    }
                }

                txtError.Visibility = Visibility.Collapsed;
                OpenRoleWindow(App.currentUser.Roles.RoleName);
                this.Close();
            
        }

        private void OpenRoleWindow(string role)
        {
            System.Windows.Window window = null;
            switch (role)
            {
                case "admin":
                    window = new AdminWindow();
                    break;
                case "moderator":
                    window = new ModeratorWindow();
                    break;
                case "user":
                    window = new UserWindow();
                    break;
                default:
                    MessageBox.Show("Неизвестная роль");
                    return;
            }
            window.Show();
        }
    }
}