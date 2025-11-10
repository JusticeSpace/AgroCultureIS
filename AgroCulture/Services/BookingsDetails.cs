using System.ComponentModel.DataAnnotations.Schema;

namespace AgroCulture
{
    /// <summary>
    /// Partial класс для View BookingsDetails
    /// ✅ Правильная обработка ФИО
    /// </summary>
    public partial class BookingsDetails
    {
        /// <summary>
        /// Имя гостя - использует NameParser для правильной сборки
        /// </summary>
        [NotMapped]
        public string GuestName
        {
            get
            {
                // Приоритет 1: Если есть GuestFullName из View
                if (!string.IsNullOrEmpty(GuestFullName))
                {
                    return GuestFullName;
                }

                // Приоритет 2: Собираем из частей (поддерживаем неполные ФИО)
                string builtName = AgroCulture.Services.NameParser.Compose(
                    GuestSurname ?? "",
                    GuestFirstName ?? "",
                    GuestMiddleName ?? "");

                return !string.IsNullOrEmpty(builtName) ? builtName : "Не указано";
            }
        }
    }
}