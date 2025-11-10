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

                    CabinsList.Clear();
                    foreach (var cabin in cabins)
                    {
                        CabinsList.Add(cabin);
                    }

                    TotalCabins = cabins.Count;
                }
            }
            catch (Exception ex)
            {
                ShowNotificationEvent($"❌ Ошибка: {ex.Message}", false);
            }
        }

        private void AddCabin()
        {
            if (string.IsNullOrWhiteSpace(NewCabinName))
            {
                ShowNotificationEvent("❌ Введите название", false);
                return;
            }

            if (NewCabinCapacity <= 0)
            {
                ShowNotificationEvent("❌ Вместимость > 0", false);
                return;
            }

            if (NewCabinPrice <= 0)
            {
                ShowNotificationEvent("❌ Цена > 0", false);
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