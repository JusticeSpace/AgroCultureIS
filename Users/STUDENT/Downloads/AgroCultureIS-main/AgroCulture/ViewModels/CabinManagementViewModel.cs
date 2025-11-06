using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AgroCulture.Commands;

namespace AgroCulture.ViewModels
{
    public class CabinManagementViewModel : BaseViewModel
    {
        private readonly AgroCultureEntities _context;

        public ObservableCollection<Cabins> Cabins { get; set; }
        public ObservableCollection<Amenities> AllAmenities { get; set; }

        // Поля формы
        private string _newCabinName;
        public string NewCabinName
        {
            get => _newCabinName;
            set { _newCabinName = value; OnPropertyChanged(); }
        }

        private string _newCabinDescription;
        public string NewCabinDescription
        {
            get => _newCabinDescription;
            set { _newCabinDescription = value; OnPropertyChanged(); }
        }

        private string _newCabinCapacity;
        public string NewCabinCapacity
        {
            get => _newCabinCapacity;
            set { _newCabinCapacity = value; OnPropertyChanged(); }
        }

        private string _newCabinPrice;
        public string NewCabinPrice
        {
            get => _newCabinPrice;
            set { _newCabinPrice = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AmenityCheckBox> SelectedAmenities { get; set; }

        public RelayCommand AddCabinCommand { get; }
        public RelayCommand<Cabins> DeleteCabinCommand { get; }

        public CabinManagementViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                // В режиме дизайнера - создаём пустые коллекции
                Cabins = new ObservableCollection<Cabins>();
                AllAmenities = new ObservableCollection<Amenities>();
                SelectedAmenities = new ObservableCollection<AmenityCheckBox>();
                return;
            }

            // Обычная инициализация для runtime
            _context = new AgroCultureEntities();

            // Загружаем домики
            Cabins = new ObservableCollection<Cabins>(
                _context.Cabins.ToList()
            );

            // Загружаем удобства
            AllAmenities = new ObservableCollection<Amenities>(
                _context.Amenities.ToList()
            );

            // CheckBox-обёртки
            SelectedAmenities = new ObservableCollection<AmenityCheckBox>(
                AllAmenities.Select(a => new AmenityCheckBox { Amenity = a, IsChecked = false })
            );

            AddCabinCommand = new RelayCommand(AddCabin, CanAddCabin);
            DeleteCabinCommand = new RelayCommand<Cabins>(DeleteCabin);
        }

        private bool CanAddCabin()
        {
            return !string.IsNullOrWhiteSpace(NewCabinName) &&
                   int.TryParse(NewCabinCapacity, out _) &&
                   decimal.TryParse(NewCabinPrice, out _);
        }

        private void AddCabin()
        {
            try
            {
                var cabin = new Cabins
                {
                    Name = NewCabinName.Trim(),
                    Description = NewCabinDescription?.Trim(),
                    Capacity = int.Parse(NewCabinCapacity),
                    PricePerNight = decimal.Parse(NewCabinPrice),
                    IsActive = true
                };

                _context.Cabins.Add(cabin);
                _context.SaveChanges();

                // Связи с удобствами
                foreach (var amenityCheckBox in SelectedAmenities.Where(a => a.IsChecked))
                {
                    _context.CabinAmenities.Add(new CabinAmenities
                    {
                        CabinId = cabin.CabinId,
                        AmenityId = amenityCheckBox.Amenity.AmenityId
                    });
                }
                _context.SaveChanges();

                Cabins.Add(cabin);
                ClearForm();

                MessageBox.Show("Домик успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCabin(Cabins cabin)
        {
            if (cabin == null) return;

            var result = MessageBox.Show(
                $"Удалить домик \"{cabin.Name}\"?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                cabin.IsActive = false;
                _context.SaveChanges();
                Cabins.Remove(cabin);
            }
        }

        private void ClearForm()
        {
            NewCabinName = string.Empty;
            NewCabinDescription = string.Empty;
            NewCabinCapacity = string.Empty;
            NewCabinPrice = string.Empty;

            foreach (var amenity in SelectedAmenities)
                amenity.IsChecked = false;
        }
    }

    public class AmenityCheckBox : BaseViewModel
    {
        public Amenities Amenity { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(); }
        }
    }
}