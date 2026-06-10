using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace SilaLesaWpfApp.View.Window
{
    public partial class LoginWindow : System.Windows.Window
    {
        public static int CurrentUserID { get; set; }
        public static string CurrentUsername { get; set; }
        public static string CurrentRole { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
            LoadRoles();
        }

        private void LoadRoles()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT RoleID, RoleName FROM Roles");
            cmbRole.ItemsSource = dt.DefaultView;
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

            DataTable dt = DatabaseHelper.ExecuteQuery(
                "SELECT u.UserID, u.Username, r.RoleName FROM AppUsers u " +
                "JOIN Roles r ON u.RoleID = r.RoleID " +
                "WHERE u.Username = @login AND u.PasswordHash = @password AND u.IsActive = 1",
                new SqlParameter("@login", login),
                new SqlParameter("@password", password));

            if (dt.Rows.Count > 0)
            {
                CurrentUserID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                CurrentUsername = dt.Rows[0]["Username"].ToString();
                CurrentRole = dt.Rows[0]["RoleName"].ToString();

                // Проверка выбранной роли
                if (cmbRole.SelectedValue != null)
                {
                    string selectedRole = ((DataRowView)cmbRole.SelectedItem)["RoleName"].ToString();
                    if (selectedRole != CurrentRole)
                    {
                        txtError.Text = "Выбранная роль не соответствует вашей учетной записи";
                        txtError.Visibility = Visibility.Visible;
                        return;
                    }
                }

                txtError.Visibility = Visibility.Collapsed;
                OpenRoleWindow(CurrentRole);
                this.Close();
            }
            else
            {
                txtError.Text = "Неверный логин или пароль";
                txtError.Visibility = Visibility.Visible;
            }
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