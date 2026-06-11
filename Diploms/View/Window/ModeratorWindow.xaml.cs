using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SilaLesaWpfApp.View.Window
{
    public partial class ModeratorWindow : System.Windows.Window
    {
        public ModeratorWindow()
        {
            InitializeComponent();
            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadCustomers(); LoadSites(); LoadServices(); LoadBookings();
        }

        private void LoadCustomers() => dgCustomers.ItemsSource = App.context.Customers.ToList();
        private void LoadSites() => dgSites.ItemsSource = App.context.Sites.Where(s => s.IsActive == true).ToList();
        private void LoadServices() => dgServices.ItemsSource = App.context.Services.Where(s => s.IsActive == true).ToList();
        private void LoadBookings() => dgBookings.ItemsSource = (from b in App.context.Bookings join c in App.context.Customers on b.CustomerID equals c.CustomerID join s in App.context.Sites on b.SiteID equals s.SiteID select new { b.BookingID, c.FullName, s.SiteName, b.CheckInDate, b.CheckOutDate, b.Status }).ToList();

        private void btnRefresh_Click(object sender, RoutedEventArgs e) => LoadAllData();

        private void btnLogout_Click(object sender, RoutedEventArgs e) { new LoginWindow().Show(); Close(); }

        private void dgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgBookings.SelectedItem == null) return;
            dynamic row = dgBookings.SelectedItem;
            string status = row.Status;
            foreach (ComboBoxItem item in cmbStatus.Items)
                if (item.Content.ToString() == status) { cmbStatus.SelectedItem = item; break; }
        }

        private void btnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (dgBookings.SelectedItem == null) { MessageBox.Show("Выберите бронирование"); return; }
            dynamic row = dgBookings.SelectedItem;
            int bookingID = row.BookingID;
            string newStatus = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();
            var booking = App.context.Bookings.Find(bookingID);
            if (booking != null) booking.Status = newStatus;
            App.context.SaveChanges();
            MessageBox.Show("Статус обновлен");
            LoadBookings();
        }
    }
}