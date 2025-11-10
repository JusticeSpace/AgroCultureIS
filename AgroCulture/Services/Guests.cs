using System.ComponentModel.DataAnnotations.Schema;

namespace AgroCulture
{
    /// <summary>
    /// Partial класс для Guests с computed свойством FullName
    /// </summary>
    public partial class Guests
    {
        /// <summary>
        /// Полное имя (ФИО) - вычисляемое свойство для обратной совместимости
        /// </summary>
        [NotMapped]
        public string FullName
        {
            get
            {
                // Проверяем, есть ли новые поля (после миграции)
                if (!string.IsNullOrEmpty(Surname) &&
                    !string.IsNullOrEmpty(FirstName) &&
                    !string.IsNullOrEmpty(MiddleName))
                {
                    return $"{Surname} {FirstName} {MiddleName}".Trim();
                }

                return "Не указано";
            }
        }
    }
}