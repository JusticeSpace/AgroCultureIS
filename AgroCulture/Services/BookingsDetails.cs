using System.ComponentModel.DataAnnotations.Schema;

namespace AgroCulture
{
    /// <summary>
    /// Partial класс для View BookingsDetails
    /// </summary>
    public partial class BookingsDetails
    {
        /// <summary>
        /// Имя гостя (для обратной совместимости)
        /// </summary>
        [NotMapped]
        public string GuestName
        {
            get
            {
                // Если есть GuestFullName из View - используем его
                if (!string.IsNullOrEmpty(GuestFullName))
                {
                    return GuestFullName;
                }

                // Fallback - собираем из частей
                if (!string.IsNullOrEmpty(GuestSurname) &&
                    !string.IsNullOrEmpty(GuestFirstName) &&
                    !string.IsNullOrEmpty(GuestMiddleName))
                {
                    return $"{GuestSurname} {GuestFirstName} {GuestMiddleName}".Trim();
                }

                return "Не указано";
            }
        }
    }
}