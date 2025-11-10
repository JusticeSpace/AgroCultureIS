using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AgroCulture.Services
{
    /// <summary>
    /// Централизованный сервис валидации для всех форм приложения
    /// </summary>
    public static class ValidationService
    {
        #region Константы валидации

        private const int MIN_USERNAME_LENGTH = 3;
        private const int MAX_USERNAME_LENGTH = 50;
        private const int MIN_PASSWORD_LENGTH = 6;
        private const int MIN_CABIN_CAPACITY = 1;
        private const int MAX_CABIN_CAPACITY = 50;
        private const decimal MIN_CABIN_PRICE = 500m;
        private const decimal MAX_CABIN_PRICE = 100000m;
        private const int MIN_CABIN_NAME_LENGTH = 3;
        private const int MAX_CABIN_NAME_LENGTH = 100;

        #endregion

        #region Валидация пользователей

        /// <summary>
        /// Проверяет логин пользователя
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return (false, "❌ Логин не может быть пустым");
            }

            username = username.Trim();

            if (username.Length < MIN_USERNAME_LENGTH)
            {
                return (false, $"❌ Логин должен содержать минимум {MIN_USERNAME_LENGTH} символа");
            }

            if (username.Length > MAX_USERNAME_LENGTH)
            {
                return (false, $"❌ Логин не может быть длиннее {MAX_USERNAME_LENGTH} символов");
            }

            // Проверка: логин не может состоять только из цифр
            if (username.All(char.IsDigit))
            {
                return (false, "❌ Логин не может состоять только из цифр");
            }

            // Проверка на допустимые символы (буквы, цифры, подчеркивание, дефис)
            if (!Regex.IsMatch(username, @"^[a-zA-Zа-яА-ЯёЁ0-9_-]+$"))
            {
                return (false, "❌ Логин может содержать только буквы, цифры, _ и -");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет пароль
        /// </summary>
        public static (bool isValid, string errorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return (false, "❌ Пароль не может быть пустым");
            }

            if (password.Length < MIN_PASSWORD_LENGTH)
            {
                return (false, $"❌ Пароль должен содержать минимум {MIN_PASSWORD_LENGTH} символов");
            }

            // Проверка наличия хотя бы одной буквы и одной цифры
            bool hasLetter = password.Any(char.IsLetter);
            bool hasDigit = password.Any(char.IsDigit);

            if (!hasLetter || !hasDigit)
            {
                return (false, "❌ Пароль должен содержать буквы и цифры");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет email
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "❌ Email не может быть пустым");
            }

            email = email.Trim();

            // Простая regex для проверки email
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            if (!Regex.IsMatch(email, emailPattern))
            {
                return (false, "❌ Некорректный формат email");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет телефон (использует существующий PhoneValidator)
        /// </summary>
        public static (bool isValid, string errorMessage) ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return (false, "❌ Телефон не может быть пустым");
            }

            // Убираем все пробелы и дефисы
            string cleanPhone = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // Проверка на только цифры и знак +
            if (!Regex.IsMatch(cleanPhone, @"^\+?\d+$"))
            {
                return (false, "❌ Телефон может содержать только цифры и +");
            }

            // Проверка длины (от 10 до 15 цифр)
            string digitsOnly = Regex.Replace(cleanPhone, @"\D", "");
            if (digitsOnly.Length < 10 || digitsOnly.Length > 15)
            {
                return (false, "❌ Телефон должен содержать от 10 до 15 цифр");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет ФИО (фамилия, имя, отчество)
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateName(string name, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, $"❌ {fieldName} не может быть пустым");
            }

            name = name.Trim();

            if (name.Length < 2)
            {
                return (false, $"❌ {fieldName} должно содержать минимум 2 символа");
            }

            // Только буквы и дефис
            if (!Regex.IsMatch(name, @"^[а-яА-ЯёЁa-zA-Z\-]+$"))
            {
                return (false, $"❌ {fieldName} может содержать только буквы и дефис");
            }

            return (true, string.Empty);
        }

        #endregion

        #region Валидация домиков

        /// <summary>
        /// Проверяет название домика
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateCabinName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return (false, "❌ Название домика не может быть пустым");
            }

            name = name.Trim();

            if (name.Length < MIN_CABIN_NAME_LENGTH)
            {
                return (false, $"❌ Название должно содержать минимум {MIN_CABIN_NAME_LENGTH} символа");
            }

            if (name.Length > MAX_CABIN_NAME_LENGTH)
            {
                return (false, $"❌ Название не может быть длиннее {MAX_CABIN_NAME_LENGTH} символов");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет вместимость домика
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateCabinCapacity(int capacity)
        {
            if (capacity < MIN_CABIN_CAPACITY)
            {
                return (false, $"❌ Вместимость не может быть меньше {MIN_CABIN_CAPACITY}");
            }

            if (capacity > MAX_CABIN_CAPACITY)
            {
                return (false, $"❌ Вместимость не может быть больше {MAX_CABIN_CAPACITY} человек (это же не отель!)");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет цену за ночь
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateCabinPrice(decimal price)
        {
            if (price < MIN_CABIN_PRICE)
            {
                return (false, $"❌ Цена не может быть меньше {MIN_CABIN_PRICE} ₽");
            }

            if (price > MAX_CABIN_PRICE)
            {
                return (false, $"❌ Цена не может быть больше {MAX_CABIN_PRICE:N0} ₽");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет все параметры домика сразу
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateCabin(string name, int capacity, decimal price)
        {
            var nameValidation = ValidateCabinName(name);
            if (!nameValidation.isValid)
                return nameValidation;

            var capacityValidation = ValidateCabinCapacity(capacity);
            if (!capacityValidation.isValid)
                return capacityValidation;

            var priceValidation = ValidateCabinPrice(price);
            if (!priceValidation.isValid)
                return priceValidation;

            return (true, string.Empty);
        }

        #endregion

        #region Валидация бронирований

        /// <summary>
        /// Проверяет даты бронирования
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateBookingDates(DateTime checkIn, DateTime checkOut)
        {
            // Проверка: дата заезда не может быть в прошлом
            if (checkIn.Date < DateTime.Now.Date)
            {
                return (false, "❌ Дата заезда не может быть в прошлом");
            }

            // Проверка: дата выезда должна быть позже даты заезда
            if (checkOut.Date <= checkIn.Date)
            {
                return (false, "❌ Дата выезда должна быть позже даты заезда");
            }

            // Проверка: бронирование не может быть слишком длинным (например, больше года)
            TimeSpan duration = checkOut - checkIn;
            if (duration.TotalDays > 365)
            {
                return (false, "❌ Бронирование не может быть длиннее 1 года");
            }

            // Проверка: бронирование должно быть минимум на 1 ночь
            if (duration.TotalDays < 1)
            {
                return (false, "❌ Бронирование должно быть минимум на 1 ночь");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет пересечение дат бронирования
        /// </summary>
        public static bool CheckBookingOverlap(
            DateTime newCheckIn,
            DateTime newCheckOut,
            DateTime existingCheckIn,
            DateTime existingCheckOut)
        {
            // Проверка пересечения периодов
            // Два периода НЕ пересекаются если:
            // - новое бронирование заканчивается до начала существующего
            // - новое бронирование начинается после окончания существующего

            bool noOverlap = (newCheckOut.Date <= existingCheckIn.Date) ||
                            (newCheckIn.Date >= existingCheckOut.Date);

            return !noOverlap; // Возвращаем true если ЕСТЬ пересечение
        }

        /// <summary>
        /// Проверяет наличие пересечений с существующими бронированиями
        /// </summary>
        public static (bool hasOverlap, string errorMessage) CheckCabinAvailability(
            int cabinId,
            DateTime checkIn,
            DateTime checkOut,
            AgroCultureEntities context,
            int? excludeBookingId = null)
        {
            try
            {
                // Получаем все активные бронирования для данного домика
                var overlappingBookings = context.Bookings
                    .Where(b => b.CabinId == cabinId)
                    .Where(b => b.Status == "active" || b.Status == "pending")
                    .Where(b => excludeBookingId == null || b.BookingId != excludeBookingId)
                    .ToList()
                    .Where(b => CheckBookingOverlap(checkIn, checkOut, b.CheckInDate, b.CheckOutDate))
                    .ToList();

                if (overlappingBookings.Any())
                {
                    var firstOverlap = overlappingBookings.First();
                    return (true, $"❌ Домик занят с {firstOverlap.CheckInDate:dd.MM.yyyy} по {firstOverlap.CheckOutDate:dd.MM.yyyy}");
                }

                return (false, string.Empty);
            }
            catch (Exception ex)
            {
                return (true, $"❌ Ошибка проверки доступности: {ex.Message}");
            }
        }

        #endregion

        #region Валидация числовых значений

        /// <summary>
        /// Проверяет что число положительное
        /// </summary>
        public static (bool isValid, string errorMessage) ValidatePositiveNumber(decimal value, string fieldName)
        {
            if (value <= 0)
            {
                return (false, $"❌ {fieldName} должно быть больше нуля");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет что число целое положительное
        /// </summary>
        public static (bool isValid, string errorMessage) ValidatePositiveInteger(int value, string fieldName)
        {
            if (value <= 0)
            {
                return (false, $"❌ {fieldName} должно быть больше нуля");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// Проверяет что число в заданном диапазоне
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateRange(
            decimal value,
            decimal min,
            decimal max,
            string fieldName)
        {
            if (value < min || value > max)
            {
                return (false, $"❌ {fieldName} должно быть в диапазоне от {min} до {max}");
            }

            return (true, string.Empty);
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Пытается распарсить строку в целое число
        /// </summary>
        public static (bool success, int value, string errorMessage) TryParseInt(string text, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return (false, 0, $"❌ {fieldName} не может быть пустым");
            }

            if (int.TryParse(text.Trim(), out int value))
            {
                return (true, value, string.Empty);
            }

            return (false, 0, $"❌ {fieldName} должно быть целым числом");
        }

        /// <summary>
        /// Пытается распарсить строку в decimal
        /// </summary>
        public static (bool success, decimal value, string errorMessage) TryParseDecimal(string text, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return (false, 0, $"❌ {fieldName} не может быть пустым");
            }

            // Заменяем запятую на точку для корректного парсинга
            string normalized = text.Trim().Replace(',', '.');

            if (decimal.TryParse(normalized, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out decimal value))
            {
                return (true, value, string.Empty);
            }

            return (false, 0, $"❌ {fieldName} должно быть числом");
        }

        #endregion
    }
}
