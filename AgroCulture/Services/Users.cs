using System.ComponentModel.DataAnnotations.Schema;

namespace AgroCulture
{
    /// <summary>
    /// Partial класс для Users с computed свойством FullName
    /// </summary>
    public partial class Users
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

                // Fallback (если .edmx ещё не обновлён)
                return "Не указано";
            }
        }

        /// <summary>
        /// ✅ НОВОЕ: Порядковый номер для отображения в таблице
        /// </summary>
        [NotMapped]
        public int RowNumber { get; set; }
    }
}