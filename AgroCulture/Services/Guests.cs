using System.ComponentModel.DataAnnotations.Schema;

namespace AgroCulture
{
    /// <summary>
    /// Partial класс для Guests с правильным FullName
    /// </summary>
    public partial class Guests
    {
        /// <summary>
        /// Полное имя (ФИО) - вычисляемое свойство
        /// ✅ Поддерживает случаи когда нет отчества
        /// </summary>
        [NotMapped]
        public string FullName
        {
            get
            {
                // Собираем только непустые поля
                if (string.IsNullOrWhiteSpace(Surname) &&
                    string.IsNullOrWhiteSpace(FirstName))
                {
                    return "Не указано";
                }

                // Используем NameParser для правильной сборки
                return AgroCulture.Services.NameParser.Compose(
                    Surname ?? "",
                    FirstName ?? "",
                    MiddleName ?? "");
            }
        }
    }
}