using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AgroCulture.ViewModels;

namespace AgroCulture.ViewModels
{
    public class CabinsManagementViewModel : BaseViewModel
    {
        private ObservableCollection<Cabins> _allCabins;
        private ObservableCollection<Cabins> _cabinsList;
        public ObservableCollection<Cabins> CabinsList
        {
            get => _cabinsList;
            set => SetProperty(ref _cabinsList, value);
        }

        private int _totalCabins;
        public int TotalCabins
        {
            get => _totalCabins;
            set => SetProperty(ref _totalCabins, value);
        }

        private int _activeCabins;
        public int ActiveCabins
        {
            get => _activeCabins;
            set => SetProperty(ref _activeCabins, value);
        }

        private int _totalCapacity;
        public int TotalCapacity
        {
            get => _totalCapacity;
            set => SetProperty(ref _totalCapacity, value);
        }

        private decimal _averagePrice;
        public decimal AveragePrice
        {
            get => _averagePrice;
            set => SetProperty(ref _averagePrice, value);
        }

        private int _totalBookings;
        public int TotalBookings
        {
            get => _totalBookings;
            set => SetProperty(ref _totalBookings, value);
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    ApplySearch();
                }
            }
        }

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

        private int _newCabinCapacity = 2;
        public int NewCabinCapacity
        {
            get => _newCabinCapacity;
            set { _newCabinCapacity = value; OnPropertyChanged(); }
        }

        private decimal _newCabinPrice = 1000;
        public decimal NewCabinPrice
        {
            get => _newCabinPrice;
            set { _newCabinPrice = value; OnPropertyChanged(); }
        }

        public ICommand AddCabinCommand { get; }
        public ICommand EditCabinCommand { get; }
        public ICommand DeleteCabinCommand { get; }

        public event Action<string, bool> ShowNotification;
        public event Action<Cabins> RequestEdit;

        public CabinsManagementViewModel()
        {
            _allCabins = new ObservableCollection<Cabins>();
            CabinsList = new ObservableCollection<Cabins>();

            AddCabinCommand = new RelayCommand(_ => AddCabin());
            EditCabinCommand = new RelayCommand<Cabins>(EditCabin);
            DeleteCabinCommand = new RelayCommand<Cabins>(DeleteCabin);

            if (App.CurrentUser?.Role?.ToLower() != "admin")
            {
                ShowNotificationEvent("⛔ Доступ запрещён!", false);
            }

            RefreshData();
        }

        public void RefreshData()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var cabins = context.Cabins
                        .Include("CabinAmenities.Amenities")
                        .OrderBy(c => c.Name)
                        .ToList();

                    _allCabins.Clear();
                    foreach (var cabin in cabins)
                    {
                        _allCabins.Add(cabin);
                    }

                    // Базовая статистика
                    TotalCabins = cabins.Count;
                    ActiveCabins = cabins.Count(c => c.IsActive);
                    TotalCapacity = cabins.Sum(c => c.MaxGuests);
                    AveragePrice = cabins.Any() ? cabins.Average(c => c.PricePerNight) : 0;

                    // Статистика бронирований
                    var bookings = context.Bookings
                        .Where(b => b.Status == "active" || b.Status == "completed")
                        .ToList();

                    TotalBookings = bookings.Count;
                    TotalRevenue = bookings
                        .Where(b => b.Status == "completed")
                        .Sum(b => (decimal?)b.TotalPrice) ?? 0;

                    ApplySearch();
                }
            }
            catch (Exception ex)
            {
                ShowNotificationEvent($"❌ Ошибка: {ex.Message}", false);
            }
        }

        private void ApplySearch()
        {
            CabinsList.Clear();

            var filtered = _allCabins.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.Trim().ToLower();
                filtered = filtered.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(query)) ||
                    (c.Description != null && c.Description.ToLower().Contains(query))
                );
            }

            foreach (var cabin in filtered)
            {
                CabinsList.Add(cabin);
            }
        }

        private void AddCabin()
        {
            // ✅ Валидация названия через ValidationService
            var nameValidation = Services.ValidationService.ValidateCabinName(NewCabinName);
            if (!nameValidation.isValid)
            {
                ShowNotificationEvent(nameValidation.errorMessage, false);
                return;
            }

            // ✅ Валидация вместимости через ValidationService
            var capacityValidation = Services.ValidationService.ValidateCabinCapacity(NewCabinCapacity);
            if (!capacityValidation.isValid)
            {
                ShowNotificationEvent(capacityValidation.errorMessage, false);
                return;
            }

            // ✅ Валидация цены через ValidationService
            var priceValidation = Services.ValidationService.ValidateCabinPrice(NewCabinPrice);
            if (!priceValidation.isValid)
            {
                ShowNotificationEvent(priceValidation.errorMessage, false);
                return;
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var cabin = new Cabins
                    {
                        Name = NewCabinName.Trim(),
                        Description = NewCabinDescription?.Trim() ?? "",
                        MaxGuests = NewCabinCapacity,
                        PricePerNight = NewCabinPrice,
                        ImageUrl = "",
                        IsActive = true
                    };

                    context.Cabins.Add(cabin);
                    context.SaveChanges();

                    ShowNotificationEvent($"✅ Домик добавлен!", true);

                    NewCabinName = string.Empty;
                    NewCabinDescription = string.Empty;
                    NewCabinCapacity = 2;
                    NewCabinPrice = 1000;

                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                ShowNotificationEvent($"❌ Ошибка: {ex.Message}", false);
            }
        }

        private void EditCabin(Cabins cabin)
        {
            if (cabin == null) return;
            RequestEdit?.Invoke(cabin);
        }

        private void DeleteCabin(Cabins cabin)
        {
            if (cabin == null) return;

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    int activeBookings = context.Bookings
                        .Count(b => b.CabinId == cabin.CabinId && b.Status == "active");

                    if (activeBookings > 0)
                    {
                        MessageBox.Show($"⛔ Активных бронирований: {activeBookings}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show(
                        $"🗑️ Удалить '{cabin.Name}'?",
                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes) return;

                    var cabinToDelete = context.Cabins.FirstOrDefault(c => c.CabinId == cabin.CabinId);
                    if (cabinToDelete != null)
                    {
                        cabinToDelete.IsActive = false;
                        context.SaveChanges();

                        ShowNotificationEvent($"✅ Домик удалён", true);
                        RefreshData();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotificationEvent($"❌ Ошибка: {ex.Message}", false);
            }
        }

        private void ShowNotificationEvent(string message, bool isSuccess)
        {
            ShowNotification?.Invoke(message, isSuccess);
        }
    }
}