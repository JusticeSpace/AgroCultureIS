
using System;
using System.Collections.Generic;
using System.Linq;

namespace AgroCulture.Services
{
    /// <summary>
    /// Правильный парсер ФИО с поддержкой неполных имён
    /// </summary>
    public static class NameParser
    {
        /// <summary>
        /// Парсит строку "Фамилия Имя Отчество" в отдельные компоненты
        /// Поддерживает неполные ФИО: "Петров", "Петров Иван", "Петров Иван Сергеевич"
        /// </summary>
        public static (string Surname, string FirstName, string MiddleName) Parse(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return ("", "", "");

            // Разбиваем по пробелам, удаляем пустые элементы
            string[] parts = fullName.Trim()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return ("", "", "");

            // Логика парсинга:
            // 1 часть → всегда фамилия
            // 2 часть → всегда имя
            // 3+ часть → отчество (берём первое слово)

            string surname = parts.Length > 0 ? parts[0].Trim() : "";
            string firstName = parts.Length > 1 ? parts[1].Trim() : "";
            string middleName = parts.Length > 2 ? parts[2].Trim() : "";

            return (surname, firstName, middleName);
        }

        /// <summary>
        /// Собирает ФИО из отдельных компонентов (с умной обработкой пустых значений)
        /// </summary>
        public static string Compose(string surname, string firstName, string middleName)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(surname))
                parts.Add(surname.Trim());
            if (!string.IsNullOrWhiteSpace(firstName))
                parts.Add(firstName.Trim());
            if (!string.IsNullOrWhiteSpace(middleName))
                parts.Add(middleName.Trim());

            return parts.Count > 0 ? string.Join(" ", parts) : "";
        }

        public static string GetSurname(string fullName)
        {
            var (surname, _, _) = Parse(fullName);
            return surname;
        }

        public static string GetFirstName(string fullName)
        {
            var (_, firstName, _) = Parse(fullName);
            return firstName;
        }

        public static string GetMiddleName(string fullName)
        {
            var (_, _, middleName) = Parse(fullName);
            return middleName;
        }
    }
}