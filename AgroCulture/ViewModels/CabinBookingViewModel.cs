using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace AgroCulture.ViewModels
{
    public class CabinBookingViewModel : BaseViewModel
    {
        // ═══════════════════════════════════════════════════════════
        // СВОЙСТВА
        // ═══════════════════════════════════════════════════════════

        private ObservableCollection<Cabins> _cabins;
        public ObservableCollection<Cabins> Cabins
        {
            get => _cabins;
            set => SetProperty(ref _cabins, value);
        }

        private Cabins _selectedCabin;
        public Cabins SelectedCabin
        {
            get => _selectedCabin;
            set
            {
                if (SetProperty(ref _selectedCabin, value))
                {
                    UpdateCabinSelection(value);
                    CalculateTotalPrice();
                }
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
            CreateBookingCommand = new RelayCommand(_ => CreateBooking());

            LoadCabins();
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
                    // ✅ ИЗМЕНЕНО: Добавлено .Include("Amenities")
                    var cabins = context.Cabins
                        .Include("Amenities")  // EF, загрузи сразу связанные удобства!
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
            SelectedCabin = cabin;
        }

        private void UpdateCabinSelection(Cabins selectedCabin)
        {
            foreach (var cabin in Cabins)
            {
                cabin.IsSelected = false;
            }

            if (selectedCabin != null)
            {
                selectedCabin.IsSelected = true;
            }
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

        private void CreateBooking()
        {
            // 1. Валидация полей
            if (!ValidateBooking())
                return;

            // 2. ✅ НОВОЕ: Проверка доступности домика
            if (!IsCabinAvailable(SelectedCabin.CabinId, CheckInDate.Value, CheckOutDate.Value))
            {
                var conflicts = GetConflictingBookings(SelectedCabin.CabinId, CheckInDate.Value, CheckOutDate.Value);

                string message = $"❌ Домик «{SelectedCabin.Name}» уже забронирован в эти даты!\n\n";
                message += "Конфликтующие бронирования:\n";

                foreach (var conflict in conflicts)
                {
                    message += $"• {conflict.CheckInDate:dd.MM.yyyy} - {conflict.CheckOutDate:dd.MM.yyyy}\n";
                }

                message += "\nПожалуйста, выберите другие даты или другой домик.";

                MessageBox.Show(message, "Домик недоступен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);

                ShowNotification?.Invoke("❌ Домик недоступен в выбранные даты", false);
                return;
            }

            // 3. Сохранение
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // Создать или найти гостя
                    var guest = context.Guests
                        .FirstOrDefault(g => g.Phone == GuestPhone.Trim());

                    if (guest == null)
                    {
                        guest = new Guests
                        {
                            FullName = GuestName.Trim(),
                            Phone = GuestPhone.Trim(),
                            Email = ""
                        };
                        context.Guests.Add(guest);
                        context.SaveChanges();
                    }
                    else
                    {
                        // Обновляем ФИО на случай, если изменилось
                        guest.FullName = GuestName.Trim();
                    }

                    // Создать бронирование
                    var booking = new Bookings
                    {
                        CabinId = SelectedCabin.CabinId,
                        GuestId = guest.GuestId,
                        CheckInDate = CheckInDate.Value,
                        CheckOutDate = CheckOutDate.Value,
                        Nights = Nights,
                        TotalPrice = TotalPrice,
                        Status = "active",
                        CreatedBy = App.CurrentUser.UserId,
                        CreatedAt = DateTime.Now
                    };

                    context.Bookings.Add(booking);
                    context.SaveChanges();

                    // Успешное уведомление
                    string successMessage =
                        $"✅ Бронирование успешно создано!\n\n" +
                        $"Домик: {SelectedCabin.Name}\n" +
                        $"Гость: {GuestName}\n" +
                        $"Период: {CheckInDate.Value:dd.MM.yyyy} - {CheckOutDate.Value:dd.MM.yyyy}\n" +
                        $"Ночей: {Nights}\n" +
                        $"Сумма: {TotalPrice:N0} ₽";

                    MessageBox.Show(successMessage, "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ShowNotification?.Invoke(
                        $"✅ Бронирование создано! Сумма: {TotalPrice:N0} ₽",
                        true);

                    // Очистить форму
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания бронирования:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                ShowNotification?.Invoke("❌ Ошибка сохранения", false);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ✅ НОВОЕ: ПРОВЕРКА ДОСТУПНОСТИ ДОМИКА
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Проверяет, доступен ли домик в указанные даты
        /// </summary>
        private bool IsCabinAvailable(int cabinId, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // Ищем пересекающиеся бронирования
                    var hasConflicts = context.Bookings
                        .Any(b =>
                            b.CabinId == cabinId &&
                            b.Status == "active" &&
                            (
                                // Новое бронирование начинается во время существующего
                                (checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                                // Новое бронирование заканчивается во время существующего
                                (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                                // Новое бронирование полностью покрывает существующее
                                (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)
                            )
                        );

                    return !hasConflicts; // Доступен, если НЕТ конфликтов
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки доступности: {ex.Message}");
                return false; // В случае ошибки считаем недоступным (безопаснее)
            }
        }

        /// <summary>
        /// Получает список конфликтующих бронирований
        /// </summary>
        private List<Bookings> GetConflictingBookings(int cabinId, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    return context.Bookings
                        .Where(b =>
                            b.CabinId == cabinId &&
                            b.Status == "active" &&
                            (
                                (checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                                (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                                (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)
                            )
                        )
                        .OrderBy(b => b.CheckInDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения конфликтов: {ex.Message}");
                return new List<Bookings>();
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ВАЛИДАЦИЯ
        // ═══════════════════════════════════════════════════════════

        private bool ValidateBooking()
        {
            if (SelectedCabin == null)
            {
                ShowNotification?.Invoke("❌ Выберите домик", false);
                MessageBox.Show("Выберите домик для бронирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(GuestName))
            {
                ShowNotification?.Invoke("❌ Введите ФИО гостя", false);
                MessageBox.Show("Введите ФИО гостя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(GuestPhone))
            {
                ShowNotification?.Invoke("❌ Введите телефон", false);
                MessageBox.Show("Введите телефон гостя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CheckInDate == null || CheckOutDate == null)
            {
                ShowNotification?.Invoke("❌ Выберите даты", false);
                MessageBox.Show("Выберите даты заезда и выезда", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CheckInDate.Value.Date < DateTime.Today)
            {
                ShowNotification?.Invoke("❌ Дата заезда в прошлом", false);
                MessageBox.Show("Дата заезда не может быть в прошлом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CheckOutDate <= CheckInDate)
            {
                ShowNotification?.Invoke("❌ Некорректные даты", false);
                MessageBox.Show("Дата выезда должна быть позже даты заезда", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (Nights <= 0)
            {
                ShowNotification?.Invoke("❌ Некорректное количество ночей", false);
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
            CheckInDate = null;
            CheckOutDate = null;
            Nights = 0;
            TotalPrice = 0;
            ShowTotalPrice = false;

            UpdateCabinSelection(null);
        }
    }
}