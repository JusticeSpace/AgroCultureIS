using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AgroCulture.Commands;

namespace AgroCulture
{
    /// <summary>
    /// Расширение класса Cabins для поддержки UI
    /// </summary>
    public partial class Cabins : INotifyPropertyChanged
    {
        /// <summary>
        /// Список удобств для отображения в XAML
        /// </summary>
        public List<Amenities> AmenitiesList
        {
            get
            {
                try
                {
                    using (var context = new AgroCultureEntities())
                    {
                        // ✅ Загружаем удобства через связующую таблицу
                        return context.CabinAmenities
                            .Where(ca => ca.CabinId == this.CabinId)
                            .Select(ca => ca.Amenities)
                            .ToList();
                    }
                }
                catch
                {
                    return new List<Amenities>();
                }
            }
        }

        // Для визуального выбора в каталоге
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}