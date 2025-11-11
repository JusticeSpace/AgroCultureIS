using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AgroCulture
{
    /// <summary>
    /// Расширение автогенерированного класса Cabins
    /// </summary>
    public partial class Cabins : INotifyPropertyChanged
    {
        // ✅ НОВОЕ: Кэшированный список удобств
        private List<string> _amenitiesListCache;
        private bool _amenitiesLoaded = false;

        /// <summary>
        /// Список удобств для отображения в UI
        /// ✅ С кэшированием для избежания множественных запросов к БД
        /// </summary>
        public List<string> AmenitiesList
        {
            get
            {
                // Если уже загружено - возвращаем кэш
                if (_amenitiesLoaded && _amenitiesListCache != null)
                {
                    return _amenitiesListCache;
                }

                // Если есть загруженные связи - используем их
                if (CabinAmenities != null && CabinAmenities.Count > 0)
                {
                    _amenitiesListCache = CabinAmenities
                        .Where(ca => ca.Amenities != null)
                        .Select(ca => ca.Amenities.Name)
                        .ToList();
                    _amenitiesLoaded = true;
                    return _amenitiesListCache;
                }

                // Иначе возвращаем пустой список (загрузка должна быть в ViewModel)
                return new List<string>();
            }
        }

        /// <summary>
        /// ✅ НОВЫЙ МЕТОД: Загрузка удобств из контекста
        /// Вызывается явно из ViewModel после загрузки домика
        /// </summary>
        public void LoadAmenities(AgroCultureEntities context)
        {
            if (_amenitiesLoaded) return;

            try
            {
                var cabin = context.Cabins
                    .Include("CabinAmenities.Amenities")
                    .FirstOrDefault(c => c.CabinId == this.CabinId);

                if (cabin?.CabinAmenities != null)
                {
                    _amenitiesListCache = cabin.CabinAmenities
                        .Where(ca => ca.Amenities != null)
                        .Select(ca => ca.Amenities.Name)
                        .ToList();
                    _amenitiesLoaded = true;
                    OnPropertyChanged(nameof(AmenitiesList));
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CABIN] Ошибка загрузки удобств: {ex.Message}");
                _amenitiesListCache = new List<string>();
            }
        }

        /// <summary>
        /// Выбран ли домик в UI
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}