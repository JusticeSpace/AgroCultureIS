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
        /// <summary>
        /// Список удобств для отображения в UI
        /// </summary>
        public List<string> AmenitiesList
        {
            get
            {
                try
                {
                    using (var context = new AgroCultureEntities())
                    {
                        var cabin = context.Cabins
                            .Include("CabinAmenities.Amenities")  // Eager loading
                            .FirstOrDefault(c => c.CabinId == this.CabinId);

                        if (cabin == null || cabin.CabinAmenities == null)
                            return new List<string>();

                        return cabin.CabinAmenities
                            .Select(ca => ca.Amenities.Name)
                            .ToList();
                    }
                }
                catch
                {
                    return new List<string>();
                }
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