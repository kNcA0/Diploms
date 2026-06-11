using Diploms.Model;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SilaLesaWpfApp.View.Window
{
    public partial class UserWindow : System.Windows.Window
    {
        private DataTable selectedServices = new DataTable();
        private int selectedSiteID = 0;

        public UserWindow()
        {
            InitializeComponent();
            LoadCustomers();
            LoadServices();
            LoadMyBookings();
            SetupSelectedServicesTable();
        }

        private void SetupSelectedServicesTable()
        {
            selectedServices.Columns.Add("ServiceID", typeof(int));
            selectedServices.Columns.Add("ServiceName", typeof(string));
            selectedServices.Columns.Add("Quantity", typeof(int));
            selectedServices.Columns.Add("PricePerDay", typeof(decimal));
        }

        private void LoadCustomers() => cmbCustomer.ItemsSource = App.context.Customers.ToList();
        private void LoadServices() => cmbService.ItemsSource = App.context.Services.Where(s => s.IsActive == true).ToList();
        private void LoadMyBookings() => dgMyBookings.ItemsSource = (from b in App.context.Bookings where b.CreatedByUserID == App.currentUser.UserID join c in App.context.Customers on b.CustomerID equals c.CustomerID join s in App.context.Sites on b.SiteID equals s.SiteID select new { b.BookingID, c.FullName, s.SiteName, b.CheckInDate, b.CheckOutDate, b.Status }).ToList();

        private void btnSearchSites_Click(object sender, RoutedEventArgs e)
        {
            if (dpCheckIn.SelectedDate == null || dpCheckOut.SelectedDate == null) { MessageBox.Show("Выберите даты заезда и выезда"); return; }
            DateTime checkIn = dpCheckIn.SelectedDate.Value;
            DateTime checkOut = dpCheckOut.SelectedDate.Value;
            if (checkOut <= checkIn) { MessageBox.Show("Дата выезда должна быть позже даты заезда"); return; }
            var busySites = App.context.Bookings.Where(b => b.Status != "Cancelled" && ((b.CheckInDate <= checkOut && b.CheckOutDate >= checkIn))).Select(b => b.SiteID).ToList();
            dgAvailableSites.ItemsSource = App.context.Sites.Where(s => s.IsActive == true && !busySites.Contains(s.SiteID)).Select(s => new { s.SiteID, s.SiteCode, s.SiteName, s.SiteType, s.Capacity, s.PricePerNight }).ToList();
        }

        private void dgAvailableSites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { if (dgAvailableSites.SelectedItem != null) selectedSiteID = (int)((dynamic)dgAvailableSites.SelectedItem).SiteID; }

        private void btnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (cmbService.SelectedValue == null) { MessageBox.Show("Выберите услугу"); return; }
            if (!int.TryParse(txtServiceQty.Text, out int qty) || qty <= 0) { MessageBox.Show("Введите корректное количество"); return; }
            var service = (Services)cmbService.SelectedItem;
            var row = selectedServices.NewRow();
            row["ServiceID"] = service.ServiceID;
            row["ServiceName"] = service.ServiceName;
            row["Quantity"] = qty;
            row["PricePerDay"] = service.PricePerDay;
            selectedServices.Rows.Add(row);
            dgSelectedServices.ItemsSource = selectedServices.DefaultView;
        }

        private void btnClearServices_Click(object sender, RoutedEventArgs e) { selectedServices.Rows.Clear(); dgSelectedServices.ItemsSource = null; }

        private void btnCreateBooking_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCustomer.SelectedValue == null) { MessageBox.Show("Выберите клиента"); return; }
            if (selectedSiteID == 0) { MessageBox.Show("Выберите место"); return; }
            if (dpCheckIn.SelectedDate == null || dpCheckOut.SelectedDate == null) { MessageBox.Show("Выберите даты"); return; }
            var booking = new Bookings
            {
                CustomerID = (int)cmbCustomer.SelectedValue,
                SiteID = selectedSiteID,
                CheckInDate = dpCheckIn.SelectedDate.Value,
                CheckOutDate = dpCheckOut.SelectedDate.Value,
                Status = "Booked",
                CreatedByUserID = App.currentUser.UserID,
                CreatedAt = DateTime.Now.Date
            };
            App.context.Bookings.Add(booking);
            App.context.SaveChanges();
            foreach (DataRow row in selectedServices.Rows)
            {
                App.context.BookingServices.Add(new BookingServices
                {
                    BookingID = booking.BookingID,
                    ServiceID = (int)row["ServiceID"],
                    Quantity = (int)row["Quantity"]
                });
            }
            App.context.SaveChanges();
            MessageBox.Show("Бронирование создано");
            selectedServices.Rows.Clear();
            dgSelectedServices.ItemsSource = null;
            selectedSiteID = 0;
            LoadMyBookings();
        }

        private void btnCancelBooking_Click(object sender, RoutedEventArgs e)
        {
            if (dgMyBookings.SelectedItem == null) { MessageBox.Show("Выберите бронирование"); return; }
            int bookingID = (int)((dynamic)dgMyBookings.SelectedItem).BookingID;
            var booking = App.context.Bookings.Find(bookingID);
            if (booking != null) booking.Status = "Cancelled";
            App.context.SaveChanges();
            MessageBox.Show("Бронирование отменено");
            LoadMyBookings();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e) { new LoginWindow().Show(); Close(); }
    }
}