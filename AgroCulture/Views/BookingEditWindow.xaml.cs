using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AgroCulture.Views
{
    public partial class BookingEditWindow : Window
    {
        public int EditBookingId { get; set; }
        public bool DialogResultSuccess { get; private set; } = false;

        private Cabins _currentCabin;

        public BookingEditWindow()
        {
            InitializeComponent();
        }

        public BookingEditWindow(int bookingId) : this()
        {
            EditBookingId = bookingId;
            Loaded += BookingEditWindow_Loaded;
        }

        private void BookingEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBookingData();
        }

        private void LoadBookingData()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var bookingDetail = context.BookingsDetails
                        .FirstOrDefault(b => b.BookingId == EditBookingId);

                    if (bookingDetail == null)
                    {
                        MessageBox.Show("Бронирование не найдено", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                        return;
                    }

                    var booking = context.Bookings.FirstOrDefault(b => b.BookingId == EditBookingId);

                    if (booking == null)
                    {
                        MessageBox.Show("Бронирование не найдено в таблице Bookings", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                        return;
                    }

                    _currentCabin = context.Cabins.FirstOrDefault(c => c.CabinId == booking.CabinId);

                    if (_currentCabin == null)
                    {
                        MessageBox.Show("Домик для данного бронирования не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                        return;
                    }

                    // Данные домика
                    TxtCabinName.Text = bookingDetail.CabinName;
                    TxtCabinDetails.Text = $"Цена: {_currentCabin.PricePerNight:N0} ₽/ночь";

                    // ✅ ИСПРАВЛЕНО: используем GuestFullName из View
                    TxtGuestName.Text = bookingDetail.GuestFullName;
                    TxtGuestPhone.Text = bookingDetail.GuestPhone;

                    // Даты
                    DateCheckIn.SelectedDate = bookingDetail.CheckInDate;
                    DateCheckOut.SelectedDate = bookingDetail.CheckOutDate;

                    // Статус
                    CmbStatus.SelectedIndex = bookingDetail.Status == "completed" ? 1 : 0;

                    CalculateTotals();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            if (_currentCabin == null || DateCheckIn.SelectedDate == null || DateCheckOut.SelectedDate == null)
            {
                TxtNights.Text = "0 ночей";
                TxtTotalPrice.Text = "0 ₽";
                return;
            }

            DateTime checkIn = DateCheckIn.SelectedDate.Value;
            DateTime checkOut = DateCheckOut.SelectedDate.Value;

            if (checkOut <= checkIn)
            {
                TxtNights.Text = "0 ночей";
                TxtTotalPrice.Text = "0 ₽";
                return;
            }

            int nights = (checkOut - checkIn).Days;
            decimal totalPrice = nights * _currentCabin.PricePerNight;

            string nightsWord = nights % 10 == 1 && nights % 100 != 11 ? "ночь" :
                               nights % 10 >= 2 && nights % 10 <= 4 && (nights % 100 < 10 || nights % 100 >= 20) ? "ночи" : "ночей";

            TxtNights.Text = $"{nights} {nightsWord}";
            TxtTotalPrice.Text = $"{totalPrice:N0} ₽";
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TxtGuestName.Text))
            {
                MessageBox.Show("Введите имя гостя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtGuestName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtGuestPhone.Text))
            {
                MessageBox.Show("Введите телефон", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtGuestPhone.Focus();
                return false;
            }

            if (DateCheckIn.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату заезда", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DateCheckOut.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату выезда", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DateCheckOut.SelectedDate <= DateCheckIn.SelectedDate)
            {
                MessageBox.Show("Дата выезда должна быть позже даты заезда", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var booking = context.Bookings.FirstOrDefault(b => b.BookingId == EditBookingId);

                    if (booking == null)
                    {
                        MessageBox.Show("Бронирование не найдено", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // ✅ ИСПРАВЛЕНО: Обновление гостя с новой структурой ФИО
                    var guest = context.Guests.FirstOrDefault(g => g.GuestId == booking.GuestId);
                    if (guest != null)
                    {
                        // Парсим ФИО из поля ввода (временное решение)
                        string fullName = TxtGuestName.Text.Trim();
                        string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        guest.Surname = parts.Length > 0 ? parts[0] : "";
                        guest.FirstName = parts.Length > 1 ? parts[1] : "";
                        guest.MiddleName = parts.Length > 2 ? parts[2] : "";
                        guest.Phone = TxtGuestPhone.Text.Trim();
                    }

                    // Обновление бронирования
                    booking.CheckInDate = DateCheckIn.SelectedDate.Value;
                    booking.CheckOutDate = DateCheckOut.SelectedDate.Value;
                    booking.Status = ((ComboBoxItem)CmbStatus.SelectedItem).Tag.ToString();

                    int nights = (booking.CheckOutDate - booking.CheckInDate).Days;
                    booking.Nights = nights;
                    booking.TotalPrice = nights * _currentCabin.PricePerNight;

                    context.SaveChanges();

                    MessageBox.Show("Бронирование успешно обновлено", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResultSuccess = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResultSuccess = false;
            this.Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResultSuccess = false;
            this.Close();
        }
    }
}