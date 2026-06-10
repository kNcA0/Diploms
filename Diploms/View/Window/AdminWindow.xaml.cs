using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace SilaLesaWpfApp.View.Window
{
    public partial class AdminWindow : System.Windows.Window
    {
        private int selectedUserID = 0;
        private int selectedCustomerID = 0;
        private int selectedSiteID = 0;
        private int selectedServiceID = 0;
        private int selectedBookingID = 0;
        private int selectedVisitID = 0;

        public AdminWindow()
        {
            InitializeComponent();
            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadRoles();
            LoadUsers();
            LoadCustomers();
            LoadSites();
            LoadServices();
            LoadBookings();
            LoadVisits();
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            cmbNewUserRole.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT RoleID, RoleName FROM Roles").DefaultView;
            cmbChangeRole.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT RoleID, RoleName FROM Roles").DefaultView;
            cmbVisitCustomer.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT CustomerID, FullName FROM Customers").DefaultView;
            cmbVisitSite.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT SiteID, SiteName FROM Sites WHERE IsActive = 1").DefaultView;
        }

        private void LoadRoles()
        {
            dgRoles.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT * FROM Roles").DefaultView;
        }

        private void LoadUsers()
        {
            dgUsers.ItemsSource = DatabaseHelper.ExecuteQuery(
                "SELECT u.UserID, u.Username, r.RoleName, u.IsActive, u.CreatedAt " +
                "FROM AppUsers u JOIN Roles r ON u.RoleID = r.RoleID").DefaultView;
        }

        private void LoadCustomers()
        {
            dgCustomers.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT * FROM Customers").DefaultView;
        }

        private void LoadSites()
        {
            dgSites.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT * FROM Sites").DefaultView;
        }

        private void LoadServices()
        {
            dgServices.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT * FROM Services").DefaultView;
        }

        private void LoadBookings()
        {
            dgBookings.ItemsSource = DatabaseHelper.ExecuteQuery(
                "SELECT b.BookingID, c.FullName, s.SiteName, b.CheckInDate, b.CheckOutDate, b.Status, u.Username as CreatedBy " +
                "FROM Bookings b " +
                "JOIN Customers c ON b.CustomerID = c.CustomerID " +
                "JOIN Sites s ON b.SiteID = s.SiteID " +
                "JOIN AppUsers u ON b.CreatedByUserID = u.UserID").DefaultView;
        }

        private void LoadVisits()
        {
            dgVisits.ItemsSource = DatabaseHelper.ExecuteQuery(
                "SELECT v.VisitID, c.FullName, s.SiteName, v.VisitStart, v.VisitEnd, v.Notes " +
                "FROM CustomerVisits v " +
                "JOIN Customers c ON v.CustomerID = c.CustomerID " +
                "JOIN Sites s ON v.SiteID = s.SiteID").DefaultView;
        }

        // === ПОЛЬЗОВАТЕЛИ И РОЛИ ===
        private void btnAddRole_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRoleName.Text.Trim()))
            {
                MessageBox.Show("Введите название роли");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Roles (RoleName) VALUES (@name)",
                new SqlParameter("@name", txtRoleName.Text.Trim()));
            txtRoleName.Clear();
            LoadRoles();
            LoadComboBoxes();
            MessageBox.Show("Роль добавлена");
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewLogin.Text.Trim()) || string.IsNullOrEmpty(txtNewPassword.Password) || cmbNewUserRole.SelectedValue == null)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO AppUsers (Username, PasswordHash, RoleID) VALUES (@login, @pass, @roleID)",
                new SqlParameter("@login", txtNewLogin.Text.Trim()),
                new SqlParameter("@pass", txtNewPassword.Password),
                new SqlParameter("@roleID", cmbNewUserRole.SelectedValue));
            txtNewLogin.Clear();
            txtNewPassword.Clear();
            LoadUsers();
            MessageBox.Show("Пользователь добавлен");
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem != null)
            {
                selectedUserID = Convert.ToInt32(((DataRowView)dgUsers.SelectedItem)["UserID"]);
            }
        }

        private void btnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUserID == 0 || cmbChangeRole.SelectedValue == null)
            {
                MessageBox.Show("Выберите пользователя и роль");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE AppUsers SET RoleID = @roleID WHERE UserID = @id",
                new SqlParameter("@roleID", cmbChangeRole.SelectedValue),
                new SqlParameter("@id", selectedUserID));
            LoadUsers();
            MessageBox.Show("Роль изменена");
        }

        private void btnToggleUser_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUserID == 0)
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE AppUsers SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE UserID = @id",
                new SqlParameter("@id", selectedUserID));
            LoadUsers();
            MessageBox.Show("Статус изменен");
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUserID == 0)
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }
            if (MessageBox.Show("Удалить пользователя?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM AppUsers WHERE UserID = @id",
                    new SqlParameter("@id", selectedUserID));
                LoadUsers();
                MessageBox.Show("Пользователь удален");
            }
        }

        // === КЛИЕНТЫ ===
        private void dgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomers.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgCustomers.SelectedItem;
                selectedCustomerID = Convert.ToInt32(row["CustomerID"]);
                txtCustomerName.Text = row["FullName"].ToString();
                txtCustomerPhone.Text = row["Phone"].ToString();
                txtCustomerEmail.Text = row["Email"]?.ToString() ?? "";
            }
        }

        private void btnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerName.Text.Trim()) || string.IsNullOrEmpty(txtCustomerPhone.Text.Trim()))
            {
                MessageBox.Show("Введите ФИО и телефон");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Customers (FullName, Phone, Email) VALUES (@name, @phone, @email)",
                new SqlParameter("@name", txtCustomerName.Text.Trim()),
                new SqlParameter("@phone", txtCustomerPhone.Text.Trim()),
                new SqlParameter("@email", txtCustomerEmail.Text.Trim()));
            ClearCustomerFields();
            LoadCustomers();
            LoadComboBoxes();
            MessageBox.Show("Клиент добавлен");
        }

        private void btnUpdateCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomerID == 0)
            {
                MessageBox.Show("Выберите клиента");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Customers SET FullName = @name, Phone = @phone, Email = @email WHERE CustomerID = @id",
                new SqlParameter("@name", txtCustomerName.Text.Trim()),
                new SqlParameter("@phone", txtCustomerPhone.Text.Trim()),
                new SqlParameter("@email", txtCustomerEmail.Text.Trim()),
                new SqlParameter("@id", selectedCustomerID));
            ClearCustomerFields();
            LoadCustomers();
            LoadComboBoxes();
            MessageBox.Show("Клиент обновлен");
        }

        private void btnDeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomerID == 0)
            {
                MessageBox.Show("Выберите клиента");
                return;
            }
            if (MessageBox.Show("Удалить клиента?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM Customers WHERE CustomerID = @id",
                    new SqlParameter("@id", selectedCustomerID));
                ClearCustomerFields();
                LoadCustomers();
                LoadComboBoxes();
                MessageBox.Show("Клиент удален");
            }
        }

        private void ClearCustomerFields()
        {
            txtCustomerName.Clear();
            txtCustomerPhone.Clear();
            txtCustomerEmail.Clear();
            selectedCustomerID = 0;
        }

        // === МЕСТА ===
        private void dgSites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSites.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgSites.SelectedItem;
                selectedSiteID = Convert.ToInt32(row["SiteID"]);
                txtSiteCode.Text = row["SiteCode"].ToString();
                txtSiteName.Text = row["SiteName"].ToString();
                txtSiteType.Text = row["SiteType"].ToString();
                txtSiteCapacity.Text = row["Capacity"].ToString();
                txtSitePrice.Text = row["PricePerNight"].ToString();
            }
        }

        private void btnAddSite_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSiteCode.Text.Trim()) || string.IsNullOrEmpty(txtSiteName.Text.Trim()))
            {
                MessageBox.Show("Введите код и название");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Sites (SiteCode, SiteName, SiteType, Capacity, PricePerNight) " +
                "VALUES (@code, @name, @type, @cap, @price)",
                new SqlParameter("@code", txtSiteCode.Text.Trim()),
                new SqlParameter("@name", txtSiteName.Text.Trim()),
                new SqlParameter("@type", txtSiteType.Text.Trim()),
                new SqlParameter("@cap", Convert.ToInt32(txtSiteCapacity.Text)),
                new SqlParameter("@price", Convert.ToDecimal(txtSitePrice.Text)));
            ClearSiteFields();
            LoadSites();
            LoadComboBoxes();
            MessageBox.Show("Место добавлено");
        }

        private void btnUpdateSite_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSiteID == 0)
            {
                MessageBox.Show("Выберите место");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Sites SET SiteCode = @code, SiteName = @name, SiteType = @type, " +
                "Capacity = @cap, PricePerNight = @price WHERE SiteID = @id",
                new SqlParameter("@code", txtSiteCode.Text.Trim()),
                new SqlParameter("@name", txtSiteName.Text.Trim()),
                new SqlParameter("@type", txtSiteType.Text.Trim()),
                new SqlParameter("@cap", Convert.ToInt32(txtSiteCapacity.Text)),
                new SqlParameter("@price", Convert.ToDecimal(txtSitePrice.Text)),
                new SqlParameter("@id", selectedSiteID));
            ClearSiteFields();
            LoadSites();
            LoadComboBoxes();
            MessageBox.Show("Место обновлено");
        }

        private void btnDeleteSite_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSiteID == 0)
            {
                MessageBox.Show("Выберите место");
                return;
            }
            if (MessageBox.Show("Удалить место?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE Sites SET IsActive = 0 WHERE SiteID = @id",
                    new SqlParameter("@id", selectedSiteID));
                ClearSiteFields();
                LoadSites();
                LoadComboBoxes();
                MessageBox.Show("Место удалено");
            }
        }

        private void ClearSiteFields()
        {
            txtSiteCode.Clear();
            txtSiteName.Clear();
            txtSiteType.Clear();
            txtSiteCapacity.Clear();
            txtSitePrice.Clear();
            selectedSiteID = 0;
        }

        // === УСЛУГИ ===
        private void dgServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgServices.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgServices.SelectedItem;
                selectedServiceID = Convert.ToInt32(row["ServiceID"]);
                txtServiceName.Text = row["ServiceName"].ToString();
                txtServiceType.Text = row["ServiceType"].ToString();
                txtServicePrice.Text = row["PricePerDay"].ToString();
            }
        }

        private void btnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtServiceName.Text.Trim()))
            {
                MessageBox.Show("Введите название услуги");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Services (ServiceName, ServiceType, PricePerDay) " +
                "VALUES (@name, @type, @price)",
                new SqlParameter("@name", txtServiceName.Text.Trim()),
                new SqlParameter("@type", txtServiceType.Text.Trim()),
                new SqlParameter("@price", Convert.ToDecimal(txtServicePrice.Text)));
            ClearServiceFields();
            LoadServices();
            MessageBox.Show("Услуга добавлена");
        }

        private void btnUpdateService_Click(object sender, RoutedEventArgs e)
        {
            if (selectedServiceID == 0)
            {
                MessageBox.Show("Выберите услугу");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Services SET ServiceName = @name, ServiceType = @type, PricePerDay = @price WHERE ServiceID = @id",
                new SqlParameter("@name", txtServiceName.Text.Trim()),
                new SqlParameter("@type", txtServiceType.Text.Trim()),
                new SqlParameter("@price", Convert.ToDecimal(txtServicePrice.Text)),
                new SqlParameter("@id", selectedServiceID));
            ClearServiceFields();
            LoadServices();
            MessageBox.Show("Услуга обновлена");
        }

        private void btnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (selectedServiceID == 0)
            {
                MessageBox.Show("Выберите услугу");
                return;
            }
            if (MessageBox.Show("Удалить услугу?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE Services SET IsActive = 0 WHERE ServiceID = @id",
                    new SqlParameter("@id", selectedServiceID));
                ClearServiceFields();
                LoadServices();
                MessageBox.Show("Услуга удалена");
            }
        }

        private void ClearServiceFields()
        {
            txtServiceName.Clear();
            txtServiceType.Clear();
            txtServicePrice.Clear();
            selectedServiceID = 0;
        }

        // === БРОНИРОВАНИЯ ===
        private void dgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgBookings.SelectedItem != null)
            {
                selectedBookingID = Convert.ToInt32(((DataRowView)dgBookings.SelectedItem)["BookingID"]);
                string status = ((DataRowView)dgBookings.SelectedItem)["Status"].ToString();
                foreach (ComboBoxItem item in cmbBookingStatus.Items)
                {
                    if (item.Content.ToString() == status)
                    {
                        cmbBookingStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void btnUpdateBookingStatus_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBookingID == 0)
            {
                MessageBox.Show("Выберите бронирование");
                return;
            }
            string newStatus = ((ComboBoxItem)cmbBookingStatus.SelectedItem).Content.ToString();
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Bookings SET Status = @status WHERE BookingID = @id",
                new SqlParameter("@status", newStatus),
                new SqlParameter("@id", selectedBookingID));
            LoadBookings();
            MessageBox.Show("Статус обновлен");
        }

        private void btnDeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBookingID == 0)
            {
                MessageBox.Show("Выберите бронирование");
                return;
            }
            if (MessageBox.Show("Удалить бронирование?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM BookingServices WHERE BookingID = @id",
                    new SqlParameter("@id", selectedBookingID));
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM Bookings WHERE BookingID = @id",
                    new SqlParameter("@id", selectedBookingID));
                LoadBookings();
                MessageBox.Show("Бронирование удалено");
            }
        }

        // === ПОСЕЩЕНИЯ ===
        private void btnAddVisit_Click(object sender, RoutedEventArgs e)
        {
            if (cmbVisitCustomer.SelectedValue == null || cmbVisitSite.SelectedValue == null ||
                dpVisitStart.SelectedDate == null || dpVisitEnd.SelectedDate == null)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO CustomerVisits (CustomerID, SiteID, VisitStart, VisitEnd) " +
                "VALUES (@customerID, @siteID, @start, @end)",
                new SqlParameter("@customerID", cmbVisitCustomer.SelectedValue),
                new SqlParameter("@siteID", cmbVisitSite.SelectedValue),
                new SqlParameter("@start", dpVisitStart.SelectedDate.Value),
                new SqlParameter("@end", dpVisitEnd.SelectedDate.Value));
            LoadVisits();
            MessageBox.Show("Посещение добавлено");
        }

        private void btnDeleteVisit_Click(object sender, RoutedEventArgs e)
        {
            if (dgVisits.SelectedItem == null)
            {
                MessageBox.Show("Выберите посещение");
                return;
            }
            int visitID = Convert.ToInt32(((DataRowView)dgVisits.SelectedItem)["VisitID"]);
            if (MessageBox.Show("Удалить посещение?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM CustomerVisits WHERE VisitID = @id",
                    new SqlParameter("@id", visitID));
                LoadVisits();
                MessageBox.Show("Посещение удалено");
            }
        }

        private void btnRefreshAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAllData();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}