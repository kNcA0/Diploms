using System;
using System.Data;
using System.Data.SqlClient;
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
            LoadCustomers();
            LoadSites();
            LoadServices();
            LoadBookings();
        }

        private void LoadCustomers()
        {
            dgCustomers.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT * FROM Customers").DefaultView;
        }

        private void LoadSites()
        {
            dgSites.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT * FROM Sites WHERE IsActive = 1").DefaultView;
        }

        private void LoadServices()
        {
            dgServices.ItemsSource = DatabaseHelper.ExecuteQuery("SELECT * FROM Services WHERE IsActive = 1").DefaultView;
        }

        private void LoadBookings()
        {
            dgBookings.ItemsSource = DatabaseHelper.ExecuteQuery(
                "SELECT b.BookingID, c.FullName, s.SiteName, b.CheckInDate, b.CheckOutDate, b.Status " +
                "FROM Bookings b " +
                "JOIN Customers c ON b.CustomerID = c.CustomerID " +
                "JOIN Sites s ON b.SiteID = s.SiteID").DefaultView;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAllData();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void dgBookings_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgBookings.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dgBookings.SelectedItem;
                string status = row["Status"].ToString();
                foreach (ComboBoxItem item in cmbStatus.Items)
                {
                    if (item.Content.ToString() == status)
                    {
                        cmbStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void btnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (dgBookings.SelectedItem == null)
            {
                MessageBox.Show("Выберите бронирование");
                return;
            }

            int bookingID = Convert.ToInt32(((DataRowView)dgBookings.SelectedItem)["BookingID"]);
            string newStatus = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Bookings SET Status = @status WHERE BookingID = @id",
                new SqlParameter("@status", newStatus),
                new SqlParameter("@id", bookingID));

            MessageBox.Show("Статус обновлен");
            LoadBookings();
        }
    }
}