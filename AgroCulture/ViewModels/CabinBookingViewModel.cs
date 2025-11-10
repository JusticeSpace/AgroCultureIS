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
            if (IsGuestMode)
            {
                MessageBox.Show(
                    "В гостевом режиме создание бронирований недоступно.\n\nОбратитесь к администратору или менеджеру.",
                    "Доступ ограничен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (!ValidateBooking())
                return;

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

                    // ─────────────────────────────────────────────────────
                    // ШАГ 1: Создать/найти гостя
                    // ─────────────────────────────────────────────────────
                    System.Diagnostics.Debug.WriteLine($"[BOOKING] Поиск гостя: {GuestPhone}");

                    var guest = context.Guests.FirstOrDefault(g => g.Phone == GuestPhone.Trim());

                    if (guest == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[BOOKING] Создание нового гостя...");

                        guest = new Guests
                        {
                            Surname = NameParser.GetSurname(GuestName.Trim()) ?? "",
                            FirstName = NameParser.GetFirstName(GuestName.Trim()) ?? "",
                            MiddleName = NameParser.GetMiddleName(GuestName.Trim()) ?? "",
                            Phone = GuestPhone.Trim(),
                            Email = ""
                        };
                        context.Guests.Add(guest);
                        context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"[BOOKING] ✅ Гость создан: ID={guest.GuestId}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[BOOKING] ✅ Гость найден: ID={guest.GuestId}");

                        // Обновить данные гостя
                        guest.Surname = NameParser.GetSurname(GuestName.Trim()) ?? "";
                        guest.FirstName = NameParser.GetFirstName(GuestName.Trim()) ?? "";
                        guest.MiddleName = NameParser.GetMiddleName(GuestName.Trim()) ?? "";
                        context.SaveChanges();
                    }

                    // ─────────────────────────────────────────────────────
                    // ШАГ 2: Создать бронирование ЧЕРЕЗ SQL
                    // ─────────────────────────────────────────────────────
                    string checkInStr = CheckInDate.Value.ToString("yyyy-MM-dd");
                    string checkOutStr = CheckOutDate.Value.ToString("yyyy-MM-dd");
                    string createdAtStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    System.Diagnostics.Debug.WriteLine("[BOOKING] ═══ Создание бронирования (SQL) ═══");
                    System.Diagnostics.Debug.WriteLine($"  CabinId: {SelectedCabin.CabinId}");
                    System.Diagnostics.Debug.WriteLine($"  GuestId: {guest.GuestId}");
                    System.Diagnostics.Debug.WriteLine($"  CheckInDate: {checkInStr}");
                    System.Diagnostics.Debug.WriteLine($"  CheckOutDate: {checkOutStr}");
                    System.Diagnostics.Debug.WriteLine($"  Nights: {Nights}");
                    System.Diagnostics.Debug.WriteLine($"  TotalPrice: {TotalPrice}");
                    System.Diagnostics.Debug.WriteLine($"  Status: active");
                    System.Diagnostics.Debug.WriteLine($"  CreatedBy: {currentUserId}");
                    System.Diagnostics.Debug.WriteLine($"  CreatedAt: {createdAtStr}");

                    // ✅ ИСПРАВЛЕННЫЙ SQL: date для CheckIn/CheckOut, datetime для CreatedAt
                    string sql = @"
                INSERT INTO Bookings 
                    (CabinId, GuestId, CheckInDate, CheckOutDate, Nights, TotalPrice, Status, CreatedBy, CreatedAt)
                VALUES 
                    (@p0, @p1, CAST(@p2 AS date), CAST(@p3 AS date), @p4, @p5, @p6, @p7, CAST(@p8 AS datetime));
                
                SELECT CAST(SCOPE_IDENTITY() as int);";

                    int newBookingId = context.Database.SqlQuery<int>(
                        sql,
                        SelectedCabin.CabinId,           // @p0
                        guest.GuestId,                   // @p1
                        checkInStr,                      // @p2 → date
                        checkOutStr,                     // @p3 → date
                        Nights,                          // @p4
                        TotalPrice,                      // @p5
                        "active",                        // @p6
                        currentUserId,                   // @p7
                        createdAtStr                     // @p8 → datetime
                    ).FirstOrDefault();

                    if (newBookingId <= 0)
                    {
                        throw new Exception("Не удалось получить ID созданного бронирования");
                    }

                    System.Diagnostics.Debug.WriteLine($"[BOOKING] ✅✅✅ УСПЕХ! BookingId={newBookingId}");

                    // ─────────────────────────────────────────────────────
                    // УСПЕХ
                    // ─────────────────────────────────────────────────────
                    string successMessage =
                        $"✅ Бронирование успешно создано!\n\n" +
                        $"📋 ID: {newBookingId}\n" +
                        $"🏠 Домик: {SelectedCabin.Name}\n" +
                        $"👤 Гость: {GuestName}\n" +
                        $"📅 Период: {CheckInDate.Value:dd.MM.yyyy} - {CheckOutDate.Value:dd.MM.yyyy}\n" +
                        $"🌙 Ночей: {Nights}\n" +
                        $"💰 Сумма: {TotalPrice:N0} ₽";

                    MessageBox.Show(successMessage, "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ShowNotification?.Invoke($"✅ Бронирование #{newBookingId} создано! Сумма: {TotalPrice:N0} ₽", true);

                    ClearForm();
                }
            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                System.Diagnostics.Debug.WriteLine($"[BOOKING] ❌ SQL ошибка: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"[BOOKING] SQL Number: {sqlEx.Number}");

                string errorMsg = sqlEx.Message;
                if (sqlEx.Number == 547) // FK constraint
                {
                    errorMsg = "Ошибка связи данных. Проверьте, что домик и пользователь существуют в БД.";
                }
                else if (sqlEx.Number == 242) // datetime conversion
                {
                    errorMsg = "Ошибка формата даты. Попробуйте выбрать другие даты.";
                }

                MessageBox.Show($"❌ Ошибка БД:\n\n{errorMsg}\n\n{sqlEx.Message}", "Ошибка",
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

            // ✅ ИЗМЕНЕНО: Валидация ФИО и телефона ТОЛЬКО для admin/manager
            if (!IsGuestMode)
            {
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