using System.Text.RegularExpressions;

namespace AgroCulture.Services
{
    /// <summary>
    /// Утилита для валидации номеров телефонов
    /// </summary>
    public static class PhoneValidator
    {
        // Регулярное выражение для российских номеров телефонов
        // Поддерживаемые форматы:
        // +7 (999) 999-99-99
        // +79999999999
        // 89999999999
        // 79999999999
        // 9999999999
        private static readonly Regex PhoneRegex = new Regex(
            @"^(\+7|8|7)?[\s\-]?\(?[0-9]{3}\)?[\s\-]?[0-9]{3}[\s\-]?[0-9]{2}[\s\-]?[0-9]{2}$",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Проверяет корректность формата номера телефона
        /// </summary>
        /// <param name="phone">Номер телефона для проверки</param>
        /// <returns>True если формат корректен, иначе False</returns>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return PhoneRegex.IsMatch(phone.Trim());
        }

        /// <summary>
        /// Нормализует номер телефона к формату +7XXXXXXXXXX
        /// </summary>
        /// <param name="phone">Исходный номер телефона</param>
        /// <returns>Нормализованный номер или исходная строка если формат некорректен</returns>
        public static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            // Убираем все нецифровые символы кроме +
            string digits = Regex.Replace(phone, @"[^\d+]", "");

            // Если начинается с 8, заменяем на +7
            if (digits.StartsWith("8"))
                digits = "+7" + digits.Substring(1);

            // Если начинается с 7, добавляем +
            if (digits.StartsWith("7"))
                digits = "+" + digits;

            // Если не начинается с +7, добавляем +7
            if (!digits.StartsWith("+7"))
                digits = "+7" + digits;

            return digits;
        }

        /// <summary>
        /// Форматирует номер телефона в читаемый вид: +7 (XXX) XXX-XX-XX
        /// </summary>
        public static string FormatPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            string normalized = NormalizePhone(phone);

            // Извлекаем цифры
            string digitsOnly = Regex.Replace(normalized, @"[^\d]", "");

            if (digitsOnly.Length == 11 && digitsOnly.StartsWith("7"))
            {
                return $"+7 ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, 2)}";
            }

            return phone; // Возвращаем исходный если не удалось отформатировать
        }
    }
}
