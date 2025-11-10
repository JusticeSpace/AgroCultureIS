using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Data.SqlTypes;
using AgroCulture.Services;

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

        // ✅ НОВОЕ: Email гостя
        private string _guestEmail;
        public string GuestEmail
        {
            get => _guestEmail;
            set => SetProperty(ref _guestEmail, value);
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

        private bool _isCreatingBooking = false;

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

        // ✅ НОВОЕ: Режим гостя
        private bool _isGuestMode;
        public bool IsGuestMode
        {
            get => _isGuestMode;
            set => SetProperty(ref _isGuestMode, value);
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

            // ✅ ИСПРАВЛЕНО: Проверка режима дизайна
            if (!IsInDesignMode())
            {
                CheckGuestMode();
                LoadCabins();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CABIN VM] Режим дизайна - пропуск загрузки данных");
            }
        }

        // ✅ НОВЫЙ МЕТОД: Проверка режима дизайна
        private bool IsInDesignMode()
        {
            return DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
        }

        // ✅ НОВЫЙ МЕТОД: Проверка гостевого режима
        private void CheckGuestMode()
        {
            try
            {
                if (App.CurrentUser != null)
                {
                    IsGuestMode = App.CurrentUser.Role.ToLower() == "guest";
                    System.Diagnostics.Debug.WriteLine($"[CABIN VM] Режим гостя: {IsGuestMode}");
                }
                else
                {
                    IsGuestMode = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CABIN VM] Ошибка проверки режима: {ex.Message}");
                IsGuestMode = false;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ЗАГРУЗКА ДОМИКОВ
        // ═══════════════════════════════════════════════════════════

        private void LoadCabins()
        {
            // ✅ Дополнительная защита
            if (IsInDesignMode())
            {
                System.Diagnostics.Debug.WriteLine("[CABIN VM] LoadCabins пропущен - режим дизайна");
                return;
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var cabins = context.Cabins
                        .Include("CabinAmenities.Amenities")
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.Name)
                        .ToList();

                    Cabins.Clear();
                    foreach (var cabin in cabins)
                    {
                        Cabins.Add(cabin);
                    }

                    System.Diagnostics.Debug.WriteLine($"[CABIN VM] Загружено {cabins.Count} домиков");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CABIN VM] ❌ Ошибка загрузки домиков: {ex.Message}");

                // В режиме дизайна НЕ показываем MessageBox
                if (!IsInDesignMode())
                {
                    MessageBox.Show($"Ошибка загрузки домиков:\n{ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
            if (_isCreatingBooking)
            {
                ShowNotification?.Invoke("⏳ Бронирование уже создается...", false);
                return;
            }

            if (IsGuestMode)
            {
                MessageBox.Show(
                    "В гостевом режиме создание бронирований недоступно.\n\n" +
                    "Обратитесь к администратору или менеджеру.",
                    "Доступ ограничен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (!ValidateBooking())
                return;

            // ✅ Проверка доступности домика через ValidationService
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var availabilityCheck = Services.ValidationService.CheckCabinAvailability(
                        SelectedCabin.CabinId,
                        CheckInDate.Value,
                        CheckOutDate.Value,
                        context,
                        null); // Новое бронирование, поэтому excludeBookingId = null

                    if (availabilityCheck.hasOverlap)
                    {
                        // Получаем детали конфликтующих бронирований для подробного отображения
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
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BOOKING] ❌ Ошибка проверки доступности: {ex.Message}");
                MessageBox.Show($"❌ Ошибка проверки доступности домика:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    if (App.CurrentUser == null)
                    {
                        MessageBox.Show("❌ Пользователь не авторизован!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int currentUserId = App.CurrentUser.UserId;
                    DateTime checkIn = CheckInDate.Value;
                    DateTime checkOut = CheckOutDate.Value;

                    System.Diagnostics.Debug.WriteLine($"[BOOKING] Поиск гостя: {GuestPhone}");

                    // ✅ ШАГ 1: Создать/найти гостя с правильным парсингом ФИО
                    var guest = context.Guests.FirstOrDefault(g => g.Phone == GuestPhone.Trim());

                    if (guest == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[BOOKING] Создание нового гостя...");

                        string guestName = GuestName?.Trim() ?? "";  // ← Защита от null

                        if (string.IsNullOrWhiteSpace(guestName) && !IsGuestMode)
                        {
                            // ошибка валидации
                            return;
                        }

                        var (surname, firstName, middleName) = NameParser.Parse(guestName);

                        guest = new Guests
                        {
                            Surname = surname,
                            FirstName = firstName,
                            MiddleName = middleName,
                            Phone = GuestPhone.Trim(),
                            Email = GuestEmail?.Trim() ?? "",
                            CreatedAt = DateTime.Now
                        };

                        context.Guests.Add(guest);
                        context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"[BOOKING] ✅ Гость создан: ID={guest.GuestId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[BOOKING] ✅ Гость найден: ID={guest.GuestId}");

                        // Обновить данные гостя
                        string guestName = GuestName?.Trim() ?? "";  // ← Защита от null

                        if (string.IsNullOrWhiteSpace(guestName) && !IsGuestMode)
                        {
                            // ошибка валидации
                            return;
                        }

                        var (surname, firstName, middleName) = NameParser.Parse(guestName);

                        guest.Surname = surname;
                        guest.FirstName = firstName;
                        guest.MiddleName = middleName;
                        guest.Phone = GuestPhone.Trim();
                        context.SaveChanges();
                    }

                    // ✅ ШАГ 2: Создать бронирование - БЕЗ SQL, через Entity Framework
                    System.Diagnostics.Debug.WriteLine("[BOOKING] ═══ Создание бронирования ═══");
                    System.Diagnostics.Debug.WriteLine($"  CabinId: {SelectedCabin.CabinId}");
                    System.Diagnostics.Debug.WriteLine($"  GuestId: {guest.GuestId}");
                    System.Diagnostics.Debug.WriteLine($"  CheckIn: {checkIn:yyyy-MM-dd}");
                    System.Diagnostics.Debug.WriteLine($"  CheckOut: {checkOut:yyyy-MM-dd}");
                    System.Diagnostics.Debug.WriteLine($"  Nights: {Nights}");
                    System.Diagnostics.Debug.WriteLine($"  TotalPrice: {TotalPrice}");

                    var booking = new Bookings
                    {
                        CabinId = SelectedCabin.CabinId,
                        GuestId = guest.GuestId,
                        CheckInDate = checkIn,
                        CheckOutDate = checkOut,
                        Nights = Nights,
                        TotalPrice = TotalPrice,
                        Status = "active",
                        CreatedBy = currentUserId,
                        CreatedAt = DateTime.Now
                    };

                    context.Bookings.Add(booking);
                    context.SaveChanges();

                    int newBookingId = booking.BookingId;

                    System.Diagnostics.Debug.WriteLine($"[BOOKING] ✅✅✅ УСПЕХ! BookingId={newBookingId}");

                    // ─────────────────────────────────────────────────────
                    // УСПЕХ
                    // ─────────────────────────────────────────────────────
                    string successMessage =
                        $"✅ Бронирование успешно создано!\n\n" +
                        $"📋 ID: {newBookingId}\n" +
                        $"🏠 Домик: {SelectedCabin.Name}\n" +
                        $"👤 Гость: {guest.FullName}\n" +
                        $"📱 Телефон: {guest.Phone}\n" +
                        $"📅 Период: {checkIn:dd.MM.yyyy} - {checkOut:dd.MM.yyyy}\n" +
                        $"🌙 Ночей: {Nights}\n" +
                        $"💰 Сумма: {TotalPrice:N0} ₽";

                    MessageBox.Show(successMessage, "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ShowNotification?.Invoke(
                        $"✅ Бронирование #{newBookingId} создано! Сумма: {TotalPrice:N0} ₽",
                        true);

                    ClearForm();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"[BOOKING] ❌ SQL ошибка: {sqlEx.Message}");

                MessageBox.Show($"❌ Ошибка БД:\n\n{sqlEx.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                ShowNotification?.Invoke("❌ Ошибка сохранения в БД", false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BOOKING] ❌ Общая ошибка: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[BOOKING] StackTrace: {ex.StackTrace}");

                MessageBox.Show($"❌ Ошибка:\n\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                ShowNotification?.Invoke("❌ Ошибка создания бронирования", false);
            }
            finally
            {
                _isCreatingBooking = false;  // ← ВЫКЛЮЧАЕМ ФЛАГ
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ВСПОМОГАТЕЛЬНЫЙ МЕТОД: Округление для SQL Server datetime
        // ═══════════════════════════════════════════════════════════
        private DateTime RoundToSqlDateTime(DateTime dateTime)
        {
            // SQL Server datetime имеет точность 3.33 мс
            // Округляем до ближайшего значения, кратного 3.33 мс

            SqlDateTime sqlDateTime = new SqlDateTime(dateTime);
            return sqlDateTime.Value;
        }

        // ═══════════════════════════════════════════════════════════
        // ПРОВЕРКА ДОСТУПНОСТИ ДОМИКА
        // ═══════════════════════════════════════════════════════════

        private bool IsCabinAvailable(int cabinId, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var hasConflicts = context.Bookings
                        .Any(b =>
                            b.CabinId == cabinId &&
                            b.Status == "active" &&
                            (
                                (checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                                (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                                (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)
                            )
                        );

                    return !hasConflicts;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CABIN VM] Ошибка проверки доступности: {ex.Message}");
                return false;
            }
        }

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
                System.Diagnostics.Debug.WriteLine($"[CABIN VM] Ошибка получения конфликтов: {ex.Message}");
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

            // ✅ ИСПРАВЛЕНО: Валидация ФИО и телефона ТОЛЬКО для admin/manager
            if (!IsGuestMode)
            {
                if (string.IsNullOrWhiteSpace(GuestName))
                {
                    ShowNotification?.Invoke("❌ Введите ФИО гостя", false);
                    MessageBox.Show("Введите ФИО гостя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // ✅ Валидация телефона через ValidationService
                var phoneValidation = Services.ValidationService.ValidatePhone(GuestPhone);
                if (!phoneValidation.isValid)
                {
                    ShowNotification?.Invoke(phoneValidation.errorMessage, false);
                    MessageBox.Show(phoneValidation.errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // ✅ Валидация Email через ValidationService (опционально)
                if (!string.IsNullOrWhiteSpace(GuestEmail))
                {
                    var emailValidation = Services.ValidationService.ValidateEmail(GuestEmail);
                    if (!emailValidation.isValid)
                    {
                        ShowNotification?.Invoke(emailValidation.errorMessage, false);
                        MessageBox.Show(emailValidation.errorMessage, "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
            }

            if (CheckInDate == null || CheckOutDate == null)
            {
                ShowNotification?.Invoke("❌ Выберите даты", false);
                MessageBox.Show("Выберите даты заезда и выезда", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // ✅ Валидация дат через ValidationService
            var dateValidation = Services.ValidationService.ValidateBookingDates(CheckInDate.Value, CheckOutDate.Value);
            if (!dateValidation.isValid)
            {
                ShowNotification?.Invoke(dateValidation.errorMessage, false);
                MessageBox.Show(dateValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
            GuestEmail = string.Empty;
            CheckInDate = null;
            CheckOutDate = null;
            Nights = 0;
            TotalPrice = 0;
            ShowTotalPrice = false;

            UpdateCabinSelection(null);
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}