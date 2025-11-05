using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AgroCulture
{
    /// <summary>
    /// Расширение автогенерированного класса Cabins
    /// (этот файл НЕ перезаписывается при обновлении EDMX)
    /// </summary>
    public partial class Cabins : INotifyPropertyChanged
    {
        // ═══════════════════════════════════════════════════════════
        // ДОПОЛНИТЕЛЬНЫЕ СВОЙСТВА ДЛЯ UI
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Список удобств для отображения в ItemsControl
        /// </summary>
        public List<string> AmenitiesList
        {
            get
            {
                // Amenities - это ICollection из автогенерированного класса
                if (Amenities == null || !Amenities.Any())
                    return new List<string>();

                return Amenities.Select(a => a.Name).ToList();
            }
        }

        /// <summary>
        /// Выбран ли домик в UI (для визуального выделения)
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // INotifyPropertyChanged (для обновления UI)
        // ═══════════════════════════════════════════════════════════

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}