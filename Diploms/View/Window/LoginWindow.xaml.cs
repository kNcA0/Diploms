using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Linq;

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
            string roleName = App.context.Roles.Find(user.RoleID).RoleName;
            OpenRoleWindow(roleName);
            Close();

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