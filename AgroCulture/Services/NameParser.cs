using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroCulture.Services
{
    /// <summary>
    /// Вспомогательный класс для парсинга ФИО
    /// </summary>
    public static class NameParser
    {
        /// <summary>
        /// Парсит строку "Фамилия Имя Отчество" в отдельные компоненты
        /// </summary>
        public static (string Surname, string FirstName, string MiddleName) Parse(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return ("", "", "");

            string[] parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string surname = parts.Length > 0 ? parts[0] : "";
            string firstName = parts.Length > 1 ? parts[1] : "";
            string middleName = parts.Length > 2 ? parts[2] : "";

            return (surname, firstName, middleName);
        }

        /// <summary>
        /// Собирает ФИО из отдельных компонентов
        /// </summary>
        public static string Compose(string surname, string firstName, string middleName)
        {
            return $"{surname} {firstName} {middleName}".Trim();
        }

        /// <summary>
        /// Извлекает фамилию
        /// </summary>
        public static string GetSurname(string fullName)
        {
            var (surname, _, _) = Parse(fullName);
            return surname;
        }

        /// <summary>
        /// Извлекает имя
        /// </summary>
        public static string GetFirstName(string fullName)
        {
            var (_, firstName, _) = Parse(fullName);
            return firstName;
        }

        /// <summary>
        /// Извлекает отчество
        /// </summary>
        public static string GetMiddleName(string fullName)
        {
            var (_, _, middleName) = Parse(fullName);
            return middleName;
        }
    }
}