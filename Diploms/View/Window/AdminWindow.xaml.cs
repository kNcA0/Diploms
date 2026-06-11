using Diploms.Model;
using System;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace SilaLesaWpfApp.View.Window
{
    public partial class AdminWindow : System.Windows.Window
    {
        private int selectedUserID, selectedCustomerID, selectedSiteID, selectedServiceID, selectedBookingID;

        public AdminWindow()
        {
            InitializeComponent();
            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadRoles(); LoadUsers(); LoadCustomers(); LoadSites(); LoadServices(); LoadBookings(); LoadVisits();
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            cmbNewUserRole.ItemsSource = cmbChangeRole.ItemsSource = App.context.Roles.ToList();
            cmbNewUserRole.DisplayMemberPath = cmbChangeRole.DisplayMemberPath = "RoleName";
            cmbNewUserRole.SelectedValuePath = cmbChangeRole.SelectedValuePath = "RoleID";

            cmbVisitCustomer.ItemsSource = App.context.Customers.ToList();
            cmbVisitCustomer.DisplayMemberPath = "FullName";
            cmbVisitCustomer.SelectedValuePath = "CustomerID";

            cmbVisitSite.ItemsSource = App.context.Sites.Where(s => s.IsActive == true).ToList();
            cmbVisitSite.DisplayMemberPath = "SiteName";
            cmbVisitSite.SelectedValuePath = "SiteID";
        }

        private void LoadRoles() => dgRoles.ItemsSource = App.context.Roles.ToList();
        private void LoadUsers() => dgUsers.ItemsSource = (from u in App.context.AppUsers join r in App.context.Roles on u.RoleID equals r.RoleID select new { u.UserID, u.Username, r.RoleName, u.IsActive, u.CreatedAt }).ToList();
        private void LoadCustomers() => dgCustomers.ItemsSource = App.context.Customers.ToList();
        private void LoadSites() => dgSites.ItemsSource = App.context.Sites.ToList();
        private void LoadServices() => dgServices.ItemsSource = App.context.Services.ToList();
        private void LoadBookings() => dgBookings.ItemsSource = (from b in App.context.Bookings join c in App.context.Customers on b.CustomerID equals c.CustomerID join s in App.context.Sites on b.SiteID equals s.SiteID join u in App.context.AppUsers on b.CreatedByUserID equals u.UserID select new { b.BookingID, c.FullName, s.SiteName, b.CheckInDate, b.CheckOutDate, b.Status, CreatedBy = u.Username }).ToList();
        private void LoadVisits() => dgVisits.ItemsSource = (from v in App.context.CustomerVisits join c in App.context.Customers on v.CustomerID equals c.CustomerID join s in App.context.Sites on v.SiteID equals s.SiteID select new { v.VisitID, c.FullName, s.SiteName, v.VisitStart, v.VisitEnd, v.Notes }).ToList();

        // Roles & Users
        private void btnAddRole_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoleName.Text)) { MessageBox.Show("Введите название роли"); return; }
            App.context.Roles.Add(new Roles { RoleName = txtRoleName.Text.Trim() });
            App.context.SaveChanges();
            txtRoleName.Clear(); LoadRoles(); LoadComboBoxes();
            MessageBox.Show("Роль добавлена");
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewLogin.Text) || string.IsNullOrEmpty(txtNewPassword.Password) || cmbNewUserRole.SelectedValue == null)
            { MessageBox.Show("Заполните все поля"); return; }
            App.context.AppUsers.Add(new AppUsers { Username = txtNewLogin.Text.Trim(), PasswordHash = txtNewPassword.Password, RoleID = (int)cmbNewUserRole.SelectedValue, IsActive = true, CreatedAt = DateTime.Now.Date });
            App.context.SaveChanges();
            txtNewLogin.Clear();
            txtNewPassword.Clear();
            LoadUsers();
            dgUsers.SelectedItem = null;
            MessageBox.Show("Пользователь добавлен");
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { if (dgUsers.SelectedItem != null) selectedUserID = (int)((dynamic)dgUsers.SelectedItem).UserID; }

        private void btnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUserID == 0 || cmbChangeRole.SelectedValue == null) { MessageBox.Show("Выберите пользователя и роль"); return; }
            var user = App.context.AppUsers.Find(selectedUserID);
            if (user != null) user.RoleID = (int)cmbChangeRole.SelectedValue;
            App.context.SaveChanges();
            LoadUsers();
            dgUsers.SelectedItem = null;
            MessageBox.Show("Роль изменена");
        }

        private void btnToggleUser_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUserID == 0) { MessageBox.Show("Выберите пользователя"); return; }
            var user = App.context.AppUsers.Find(selectedUserID);
            if (user != null) user.IsActive = !user.IsActive;
            App.context.SaveChanges();
            LoadUsers();
            MessageBox.Show("Статус изменен");
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUserID == 0) { MessageBox.Show("Выберите пользователя"); return; }
            if (MessageBox.Show("Удалить пользователя?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            var user = App.context.AppUsers.Find(selectedUserID);
            if (user != null) App.context.AppUsers.Remove(user);
            App.context.SaveChanges();
            LoadUsers();
            dgUsers.SelectedItem = null;
            MessageBox.Show("Пользователь удален");
        }

        // Customers
        private void dgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomers.SelectedItem != null)
            {
                var c = (Customers)dgCustomers.SelectedItem;
                selectedCustomerID = c.CustomerID;
                txtCustomerName.Text = c.FullName; txtCustomerPhone.Text = c.Phone; txtCustomerEmail.Text = c.Email;
            }
        }

        private void btnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerName.Text) || string.IsNullOrEmpty(txtCustomerPhone.Text)) { MessageBox.Show("Введите ФИО и телефон"); return; }
            App.context.Customers.Add(new Customers { FullName = txtCustomerName.Text.Trim(), Phone = txtCustomerPhone.Text.Trim(), Email = txtCustomerEmail.Text.Trim() });
            App.context.SaveChanges();
            ClearCustomerFields();
            LoadCustomers();
            LoadComboBoxes();
            dgCustomers.SelectedItem = null;
            MessageBox.Show("Клиент добавлен");
        }

        private void btnUpdateCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomerID == 0) { MessageBox.Show("Выберите клиента"); return; }
            var c = App.context.Customers.Find(selectedCustomerID);
            if (c != null) { c.FullName = txtCustomerName.Text.Trim(); c.Phone = txtCustomerPhone.Text.Trim(); c.Email = txtCustomerEmail.Text.Trim(); }
            App.context.SaveChanges();
            ClearCustomerFields();
            LoadCustomers();
            LoadComboBoxes();
            dgCustomers.SelectedItem = null;
            MessageBox.Show("Клиент обновлен");
        }

        private void btnDeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCustomerID == 0) { MessageBox.Show("Выберите клиента"); return; }
            if (MessageBox.Show("Удалить клиента?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            var c = App.context.Customers.Find(selectedCustomerID);
            if (c != null) App.context.Customers.Remove(c);
            App.context.SaveChanges();
            ClearCustomerFields();
            LoadCustomers();
            LoadComboBoxes();
            dgCustomers.SelectedItem = null;
            MessageBox.Show("Клиент удален");
        }

        private void ClearCustomerFields()
        { 
            txtCustomerName.Clear();
            txtCustomerPhone.Clear();
            txtCustomerEmail.Clear();
            selectedCustomerID = 0;
        }

        // Sites
        private void dgSites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSites.SelectedItem != null)
            {
                var s = (Sites)dgSites.SelectedItem;
                selectedSiteID = s.SiteID;
                txtSiteCode.Text = s.SiteCode; txtSiteName.Text = s.SiteName; txtSiteType.Text = s.SiteType;
                txtSiteCapacity.Text = s.Capacity.ToString(); txtSitePrice.Text = s.PricePerNight.ToString();
            }
        }

        private void btnAddSite_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSiteCode.Text) || string.IsNullOrEmpty(txtSiteName.Text)) { MessageBox.Show("Введите код и название"); return; }
            App.context.Sites.Add(new Sites { SiteCode = txtSiteCode.Text.Trim(), SiteName = txtSiteName.Text.Trim(), SiteType = txtSiteType.Text.Trim(), Capacity = int.Parse(txtSiteCapacity.Text), PricePerNight = decimal.Parse(txtSitePrice.Text), IsActive = true });
            App.context.SaveChanges();
            ClearSiteFields();
            LoadSites();
            LoadComboBoxes();
            dgSites.SelectedItem = null;
            MessageBox.Show("Место добавлено");
        }

        private void btnUpdateSite_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSiteID == 0) { MessageBox.Show("Выберите место"); return; }
            var site = App.context.Sites.Find(selectedSiteID);
            if (site != null) { site.SiteCode = txtSiteCode.Text.Trim(); site.SiteName = txtSiteName.Text.Trim(); site.SiteType = txtSiteType.Text.Trim(); site.Capacity = int.Parse(txtSiteCapacity.Text); site.PricePerNight = decimal.Parse(txtSitePrice.Text); }
            App.context.SaveChanges();
            ClearSiteFields();
            LoadSites();
            LoadComboBoxes();
            dgSites.SelectedItem = null;
            MessageBox.Show("Место обновлено");
        }

        private void btnDeleteSite_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSiteID == 0) { MessageBox.Show("Выберите место"); return; }
            if (MessageBox.Show("Удалить место?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            var site = App.context.Sites.Find(selectedSiteID);
            if (site != null) site.IsActive = false;
            App.context.SaveChanges();
            ClearSiteFields();
            LoadSites();
            LoadComboBoxes();
            dgSites.SelectedItem = null;
            MessageBox.Show("Место удалено");
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

        // Services
        private void dgServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgServices.SelectedItem != null)
            {
                var s = (Services)dgServices.SelectedItem;
                selectedServiceID = s.ServiceID;
                txtServiceName.Text = s.ServiceName; txtServiceType.Text = s.ServiceType; txtServicePrice.Text = s.PricePerDay.ToString();
            }
        }

        private void btnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtServiceName.Text)) { MessageBox.Show("Введите название услуги"); return; }
            App.context.Services.Add(new Services { ServiceName = txtServiceName.Text.Trim(), ServiceType = txtServiceType.Text.Trim(), PricePerDay = decimal.Parse(txtServicePrice.Text), IsActive = true });
            App.context.SaveChanges();
            ClearServiceFields();
            LoadServices();
            dgServices.SelectedItem = null;
            MessageBox.Show("Услуга добавлена");
        }

        private void btnUpdateService_Click(object sender, RoutedEventArgs e)
        {
            if (selectedServiceID == 0) { MessageBox.Show("Выберите услугу"); return; }
            var svc = App.context.Services.Find(selectedServiceID);
            if (svc != null) { svc.ServiceName = txtServiceName.Text.Trim(); svc.ServiceType = txtServiceType.Text.Trim(); svc.PricePerDay = decimal.Parse(txtServicePrice.Text); }
            App.context.SaveChanges();
            ClearServiceFields();
            LoadServices();
            dgServices.SelectedItem = null;
            MessageBox.Show("Услуга обновлена");
        }

        private void btnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            if (selectedServiceID == 0) { MessageBox.Show("Выберите услугу"); return; }
            if (MessageBox.Show("Удалить услугу?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            var svc = App.context.Services.Find(selectedServiceID);
            if (svc != null) svc.IsActive = false;
            App.context.SaveChanges();
            ClearServiceFields();
            LoadServices();
            dgServices.SelectedItem = null;
            MessageBox.Show("Услуга удалена");
        }

        private void ClearServiceFields()
        {
            txtServiceName.Clear();
            txtServiceType.Clear();
            txtServicePrice.Clear();
            selectedServiceID = 0;
        }

        // Bookings
        private void dgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgBookings.SelectedItem != null)
            {
                dynamic row = dgBookings.SelectedItem;
                selectedBookingID = row.BookingID;
                string status = row.Status;
                foreach (ComboBoxItem item in cmbBookingStatus.Items)
                    if (item.Content.ToString() == status) { cmbBookingStatus.SelectedItem = item; break; }
            }
        }

        private void btnUpdateBookingStatus_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBookingID == 0) { MessageBox.Show("Выберите бронирование"); return; }
            var b = App.context.Bookings.Find(selectedBookingID);
            if (b != null) b.Status = ((ComboBoxItem)cmbBookingStatus.SelectedItem).Content.ToString();
            App.context.SaveChanges();
            LoadBookings();
            dgBookings.SelectedItem = null;
            MessageBox.Show("Статус обновлен");
        }

        private void btnDeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBookingID == 0) { MessageBox.Show("Выберите бронирование"); return; }
            if (MessageBox.Show("Удалить бронирование?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var b = App.context.Bookings.Include("BookingServices").FirstOrDefault(x => x.BookingID == selectedBookingID);

            if (b != null)
            {
                // Удаляем связанные услуги через цикл (вместо RemoveRange)
                foreach (var bs in b.BookingServices.ToList())
                {
                    App.context.BookingServices.Remove(bs);
                }

                App.context.Bookings.Remove(b);
            }

            App.context.SaveChanges();
            LoadBookings();
            dgBookings.SelectedItem = null;
            MessageBox.Show("Бронирование удалено");
        }

        // Visits
        private void btnAddVisit_Click(object sender, RoutedEventArgs e)
        {
            if (cmbVisitCustomer.SelectedValue == null || cmbVisitSite.SelectedValue == null || dpVisitStart.SelectedDate == null || dpVisitEnd.SelectedDate == null)
            { MessageBox.Show("Заполните все поля"); return; }
            App.context.CustomerVisits.Add(new CustomerVisits { CustomerID = (int)cmbVisitCustomer.SelectedValue, SiteID = (int)cmbVisitSite.SelectedValue, VisitStart = dpVisitStart.SelectedDate.Value, VisitEnd = dpVisitEnd.SelectedDate.Value });
            App.context.SaveChanges();
            LoadVisits();
            MessageBox.Show("Посещение добавлено");
        }

        private void btnDeleteVisit_Click(object sender, RoutedEventArgs e)
        {
            if (dgVisits.SelectedItem == null) { MessageBox.Show("Выберите посещение"); return; }
            int visitID = (int)((dynamic)dgVisits.SelectedItem).VisitID;
            if (MessageBox.Show("Удалить посещение?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            var v = App.context.CustomerVisits.Find(visitID);
            if (v != null) App.context.CustomerVisits.Remove(v);
            App.context.SaveChanges();
            LoadVisits();
            MessageBox.Show("Посещение удалено");
        }

        private void btnRefreshAll_Click(object sender, RoutedEventArgs e) => LoadAllData();
        private void btnLogout_Click(object sender, RoutedEventArgs e) { new LoginWindow().Show(); Close(); }
    }
}