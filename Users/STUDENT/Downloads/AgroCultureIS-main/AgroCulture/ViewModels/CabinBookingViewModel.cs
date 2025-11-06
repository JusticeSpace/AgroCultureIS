using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AgroCulture.Commands;

namespace AgroCulture.ViewModels
{
    public class CabinBookingViewModel : BaseViewModel
    {
        // ═══════════════════════════════════════════════════════════
        // СВОЙСТВА
        // ═══════════════════════════════════════════════════════════

        public ObservableCollection<Cabins> Cabins { get; set; }

        private Cabins _selectedCabin;
        public Cabins SelectedCabin
        {
            get => _selectedCabin;
            set
            {
                // Сбрасываем предыдущий выбор
                if (_selectedCabin != null)
                    _selectedCabin.IsSelected = false;

                _selectedCabin = value;

                // Устанавливаем новый выбор
                if (_selectedCabin != null)
                    _selectedCabin.IsSelected = true;

                OnPropertyChanged();
                CalculateTotalPrice();
            }
        }

        private string _guestName;
        public string GuestName
        {
            get => _guestName;
            set => SetProperty(ref _guestName, value);
        }

        private string _guestPhone;
        public string GuestPhone
        {
            get => _guestPhone;
            set => SetProperty(ref _guestPhone, value);
        }

        private DateTime? _checkInDate;
        public DateTime? CheckInDate
        {
            get => _checkInDate;
            set
            {
                if (SetProperty(ref _checkInDate, value))
                {
                    CalculateTotalPrice();
                }
            }
        }

        private DateTime? _checkOutDate;
        public DateTime? CheckOutDate
        {
            get => _checkOutDate;
            set
            {
                if (SetProperty(ref _checkOutDate, value))
                {
                    CalculateTotalPrice();
                }
            }
        }

        private int _nights;
        public int Nights
        {
            get => _nights;
            set => SetProperty(ref _nights, value);
        }

        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }

        private bool _showTotalPrice;
        public bool ShowTotalPrice
        {
            get => _showTotalPrice;
            set => SetProperty(ref _showTotalPrice, value);
        }

        // ═══════════════════════════════════════════════════════════
        // КОМАНДЫ
        // ═══════════════════════════════════════════════════════════

        public RelayCommand<Cabins> SelectCabinCommand { get; }
        public RelayCommand CreateBookingCommand { get; }

        // ═══════════════════════════════════════════════════════════
        // СОБЫТИЯ
        // ═══════════════════════════════════════════════════════════

        public event Action<string, bool> ShowNotification;

        // ═══════════════════════════════════════════════════════════
        // КОНСТРУКТОР
        // ═══════════════════════════════════════════════════════════

        public CabinBookingViewModel()
        {
            Cabins = new ObservableCollection<Cabins>();

            SelectCabinCommand = new RelayCommand<Cabins>(SelectCabin);
            CreateBookingCommand = new RelayCommand(CreateBooking, CanCreateBooking);

            LoadCabins();

            // Начальные даты
            CheckInDate = DateTime.Today;
            CheckOutDate = DateTime.Today.AddDays(1);
        }

        // ═══════════════════════════════════════════════════════════
        // ЗАГРУЗКА ДОМИКОВ
        // ═══════════════════════════════════════════════════════════

        private void LoadCabins()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var cabins = context.Cabins
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.Name)
                        .ToList();

                    Cabins.Clear();
                    foreach (var cabin in cabins)
                    {
                        Cabins.Add(cabin);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки домиков:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ВЫБОР ДОМИКА
        // ═══════════════════════════════════════════════════════════

        private void SelectCabin(Cabins cabin)
        {
            if (cabin == null) return;

            // Сбрасываем выбор у всех
            foreach (var c in Cabins)
                c.IsSelected = false;

            // Выбираем текущий
            cabin.IsSelected = true;
            SelectedCabin = cabin;
        }

        // ═══════════════════════════════════════════════════════════
        // РАСЧЕТ СТОИМОСТИ
        // ═══════════════════════════════════════════════════════════

        private void CalculateTotalPrice()
        {
            if (SelectedCabin == null || CheckInDate == null || CheckOutDate == null)
            {
                Nights = 0;
                TotalPrice = 0;
                ShowTotalPrice = false;
                return;
            }

            if (CheckOutDate <= CheckInDate)
            {
                Nights = 0;
                TotalPrice = 0;
                ShowTotalPrice = false;
                return;
            }

            Nights = (CheckOutDate.Value - CheckInDate.Value).Days;
            TotalPrice = Nights * SelectedCabin.PricePerNight;
            ShowTotalPrice = Nights > 0;
        }

        // ═══════════════════════════════════════════════════════════
        // СОЗДАНИЕ БРОНИРОВАНИЯ
        // ═══════════════════════════════════════════════════════════

        private bool CanCreateBooking()
        {
            return SelectedCabin != null &&
                   !string.IsNullOrWhiteSpace(GuestName) &&
                   !string.IsNullOrWhiteSpace(GuestPhone) &&
                   CheckInDate.HasValue &&
                   CheckOutDate.HasValue &&
                   CheckOutDate.Value > CheckInDate.Value;
        }

        private void CreateBooking()
        {
            // Валидация
            if (!ValidateBooking())
                return;

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // ✅ СОЗДАЕМ ГОСТЯ
                    var guest = new Guests
                    {
                        FullName = GuestName.Trim(),
                        Phone = GuestPhone.Trim(),
                        Email = null,
                        CreatedAt = DateTime.Now
                    };

                    context.Guests.Add(guest);

                    // ✅ СОЗДАЕМ БРОНИРОВАНИЕ
                    var booking = new Bookings
                    {
                        CabinId = SelectedCabin.CabinId,
                        Guests = guest,
                        CheckInDate = CheckInDate.Value,
                        CheckOutDate = CheckOutDate.Value,
                        Nights = Nights,
                        TotalPrice = TotalPrice,
                        Status = "active",
                        CreatedBy = App.CurrentUser?.UserId ?? 1,
                        CreatedAt = DateTime.Now
                    };

                    context.Bookings.Add(booking);
                    context.SaveChanges();

                    ShowNotification?.Invoke(
                        $"✅ Бронирование создано!\n\n" +
                        $"Домик: {SelectedCabin.Name}\n" +
                        $"Гость: {GuestName}\n" +
                        $"Период: {CheckInDate:dd.MM.yyyy} - {CheckOutDate:dd.MM.yyyy}\n" +
                        $"Ночей: {Nights}\n" +
                        $"Сумма: {TotalPrice:N0} ₽",
                        true
                    );

                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                ShowNotification?.Invoke($"Ошибка: {ex.Message}", false);
                MessageBox.Show($"Ошибка создания бронирования:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateBooking()
        {
            if (SelectedCabin == null)
            {
                ShowNotification?.Invoke("❌ Выберите домик", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(GuestName))
            {
                ShowNotification?.Invoke("❌ Введите ФИО гостя", false);
                return false;
            }

            if (string.IsNullOrWhiteSpace(GuestPhone))
            {
                ShowNotification?.Invoke("❌ Введите телефон", false);
                return false;
            }

            if (CheckInDate == null || CheckOutDate == null)
            {
                ShowNotification?.Invoke("❌ Выберите даты", false);
                return false;
            }

            if (CheckInDate.Value.Date < DateTime.Today)
            {
                ShowNotification?.Invoke("❌ Дата заезда в прошлом", false);
                return false;
            }

            if (CheckOutDate <= CheckInDate)
            {
                ShowNotification?.Invoke("❌ Некорректные даты", false);
                return false;
            }

            return true;
        }

        // ═══════════════════════════════════════════════════════════
        // ОЧИСТКА ФОРМЫ
        // ═══════════════════════════════════════════════════════════

        private void ClearForm()
        {
            SelectedCabin = null;
            GuestName = string.Empty;
            GuestPhone = string.Empty;
            CheckInDate = DateTime.Today;
            CheckOutDate = DateTime.Today.AddDays(1);

            foreach (var cabin in Cabins)
            {
                cabin.IsSelected = false;
            }
        }
    }
}