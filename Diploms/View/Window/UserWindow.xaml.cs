using System;
using System.Data;
using System.Data.SqlClient;
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

        private void LoadCustomers()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT CustomerID, FullName FROM Customers");
            cmbCustomer.ItemsSource = dt.DefaultView;
        }

        private void LoadServices()
        {
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT ServiceID, ServiceName FROM Services WHERE IsActive = 1");
            cmbService.ItemsSource = dt.DefaultView;
        }

        private void LoadMyBookings()
        {
            dgMyBookings.ItemsSource = DatabaseHelper.ExecuteQuery(
                "SELECT b.BookingID, c.FullName, s.SiteName, b.CheckInDate, b.CheckOutDate, b.Status " +
                "FROM Bookings b " +
                "JOIN Customers c ON b.CustomerID = c.CustomerID " +
                "JOIN Sites s ON b.SiteID = s.SiteID " +
                "WHERE b.CreatedByUserID = @userID",
                new SqlParameter("@userID", LoginWindow.CurrentUserID)).DefaultView;
        }

        private void btnSearchSites_Click(object sender, RoutedEventArgs e)
        {
            if (dpCheckIn.SelectedDate == null || dpCheckOut.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты заезда и выезда");
                return;
            }

            DateTime checkIn = dpCheckIn.SelectedDate.Value;
            DateTime checkOut = dpCheckOut.SelectedDate.Value;

            if (checkOut <= checkIn)
            {
                MessageBox.Show("Дата выезда должна быть позже даты заезда");
                return;
            }

            dgAvailableSites.ItemsSource = DatabaseHelper.ExecuteQuery(
                "SELECT SiteID, SiteCode, SiteName, SiteType, Capacity, PricePerNight " +
                "FROM Sites " +
                "WHERE IsActive = 1 AND SiteID NOT IN (" +
                "SELECT SiteID FROM Bookings " +
                "WHERE Status != 'Cancelled' " +
                "AND ((CheckInDate <= @checkOut AND CheckOutDate >= @checkIn)))",
                new SqlParameter("@checkIn", checkIn),
                new SqlParameter("@checkOut", checkOut)).DefaultView;
        }

        private void dgAvailableSites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAvailableSites.SelectedItem != null)
            {
                selectedSiteID = Convert.ToInt32(((DataRowView)dgAvailableSites.SelectedItem)["SiteID"]);
            }
        }

        private void btnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (cmbService.SelectedValue == null)
            {
                MessageBox.Show("Выберите услугу");
                return;
            }

            int qty;
            if (!int.TryParse(txtServiceQty.Text, out qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество");
                return;
            }

            DataRowView service = (DataRowView)cmbService.SelectedItem;
            DataRow row = selectedServices.NewRow();
            row["ServiceID"] = service["ServiceID"];
            row["ServiceName"] = service["ServiceName"];
            row["Quantity"] = qty;

            DataTable priceDt = DatabaseHelper.ExecuteQuery(
                "SELECT PricePerDay FROM Services WHERE ServiceID = @id",
                new SqlParameter("@id", service["ServiceID"]));
            row["PricePerDay"] = priceDt.Rows[0]["PricePerDay"];

            selectedServices.Rows.Add(row);
            dgSelectedServices.ItemsSource = selectedServices.DefaultView;
        }

        private void btnClearServices_Click(object sender, RoutedEventArgs e)
        {
            selectedServices.Rows.Clear();
            dgSelectedServices.ItemsSource = null;
        }

        private void btnCreateBooking_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCustomer.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента");
                return;
            }
            if (selectedSiteID == 0)
            {
                MessageBox.Show("Выберите место");
                return;
            }
            if (dpCheckIn.SelectedDate == null || dpCheckOut.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты");
                return;
            }

            int customerID = Convert.ToInt32(cmbCustomer.SelectedValue);
            DateTime checkIn = dpCheckIn.SelectedDate.Value;
            DateTime checkOut = dpCheckOut.SelectedDate.Value;

            // Создаем бронирование
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO Bookings (CustomerID, SiteID, CheckInDate, CheckOutDate, CreatedByUserID) " +
                "VALUES (@customerID, @siteID, @checkIn, @checkOut, @userID)",
                new SqlParameter("@customerID", customerID),
                new SqlParameter("@siteID", selectedSiteID),
                new SqlParameter("@checkIn", checkIn),
                new SqlParameter("@checkOut", checkOut),
                new SqlParameter("@userID", LoginWindow.CurrentUserID));

            // Получаем ID созданного бронирования
            int bookingID = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT MAX(BookingID) FROM Bookings"));

            // Добавляем услуги
            foreach (DataRow row in selectedServices.Rows)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO BookingServices (BookingID, ServiceID, Quantity) " +
                    "VALUES (@bookingID, @serviceID, @qty)",
                    new SqlParameter("@bookingID", bookingID),
                    new SqlParameter("@serviceID", row["ServiceID"]),
                    new SqlParameter("@qty", row["Quantity"]));
            }

            MessageBox.Show("Бронирование создано");
            selectedServices.Rows.Clear();
            dgSelectedServices.ItemsSource = null;
            selectedSiteID = 0;
            LoadMyBookings();
        }

        private void btnCancelBooking_Click(object sender, RoutedEventArgs e)
        {
            if (dgMyBookings.SelectedItem == null)
            {
                MessageBox.Show("Выберите бронирование");
                return;
            }

            int bookingID = Convert.ToInt32(((DataRowView)dgMyBookings.SelectedItem)["BookingID"]);

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Bookings SET Status = 'Cancelled' WHERE BookingID = @id",
                new SqlParameter("@id", bookingID));

            MessageBox.Show("Бронирование отменено");
            LoadMyBookings();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}