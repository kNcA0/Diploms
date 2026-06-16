using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Linq;
using Diploms.Model;

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
            cmbRole.ItemsSource = App.context.Roles.ToList();
            cmbRole.DisplayMemberPath = "RoleName";
            cmbRole.SelectedValuePath = "RoleID";
            cmbRole.SelectedIndex = 0;
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
            var user = App.context.AppUsers.FirstOrDefault(u => u.Username == login && u.PasswordHash == password);
            if (user == null)
            {
                txtError.Text = "Неверный логин или пароль";
                txtError.Visibility = Visibility.Visible;
                return;
            }
            if (cmbRole.SelectedValue != null)
            {
                int selectedRoleID = (int)cmbRole.SelectedValue;
                if (user.RoleID != selectedRoleID)
                {
                    txtError.Text = "Выбранная роль не соответствует вашей учетной записи";
                    txtError.Visibility = Visibility.Visible;
                    return;
                }
            }

            App.currentUser = user;
            txtError.Visibility = Visibility.Collapsed;
            Roles role = cmbRole.SelectedItem as Roles;
            if (role.RoleID == 1)
            {
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.Show();
            }
            else if (role.RoleID == 2)
            {
                ModeratorWindow moderatorWindow = new ModeratorWindow();
                moderatorWindow.Show();
            }
            else
            {
                UserWindow userWindow = new UserWindow();
                userWindow.Show();
            }

            Close();

        }
    }
}