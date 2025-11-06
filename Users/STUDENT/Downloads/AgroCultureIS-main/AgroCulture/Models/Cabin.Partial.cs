using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AgroCulture
{
    /// <summary>
    /// Расширение автосгенерированного класса Cabin из EF
    /// </summary>
    public partial class Cabin : INotifyPropertyChanged
    {
        /// <summary>
        /// Список удобств для отображения в XAML
        /// </summary>
        public List<Amenity> AmenitiesList
        {
            get
            {
                try
                {
                    using (var context = new AgroCultureEntities())
                    {
                        // ✅ Навигационное свойство уже есть в EF модели
                        return context.CabinAmenity
                            .Where(ca => ca.CabinId == this.CabinId)
                            .Select(ca => ca.Amenity)
                            .ToList();
                    }
                }
                catch
                {
                    return new List<Amenity>();
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