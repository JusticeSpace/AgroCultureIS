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
        // ════════════════════════════════════════════════════════════
        // ДАННЫЕ
        // ════════════════════════════════════════════════════════════

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

        // ════════════════════════════════════════════════════════════
        // ФОРМА ДОБАВЛЕНИЯ ДОМИКА
        // ════════════════════════════════════════════════════════════

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

        // ════════════════════════════════════════════════════════════
        // КОМАНДЫ
        // ════════════════════════════════════════════════════════════

        public ICommand AddCabinCommand { get; }
        public ICommand EditCabinCommand { get; }
        public ICommand DeleteCabinCommand { get; }

        // ════════════════════════════════════════════════════════════
        // СОБЫТИЯ
        // ════════════════════════════════════════════════════════════

        public event Action<string, bool> ShowNotification;
        public event Action<Cabins> RequestEdit;

        // ════════════════════════════════════════════════════════════
        // КОНСТРУКТОР
        // ════════════════════════════════════════════════════════════

        public CabinsManagementViewModel()
        {
            CabinsList = new ObservableCollection<Cabins>();

            AddCabinCommand = new RelayCommand(_ => AddCabin());
            EditCabinCommand = new RelayCommand<Cabins>(EditCabin);
            DeleteCabinCommand = new RelayCommand<Cabins>(DeleteCabin);

            // Проверка: только администратор может это использовать!
            if (App.CurrentUser?.Role?.ToLower() != "admin")
            {
                ShowNotificationEvent("⛔ Доступ запрещён! Только администратор может управлять домиками", false);
            }

            RefreshData();
        }

        // ════════════════════════════════════════════════════════════
        // ЗАГРУЗКА ДАННЫХ
        // ════════════════════════════════════════════════════════════

        public void RefreshData()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var cabins = context.Cabins
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
                ShowNotificationEvent($"Ошибка загрузки: {ex.Message}", false);
            }
        }

        // ════════════════════════════════════════════════════════════
        // ДОБАВЛЕНИЕ ДОМИКА
        // ════════════════════════════════════════════════════════════

        private void AddCabin()
        {
            if (string.IsNullOrWhiteSpace(NewCabinName))
            {
                ShowNotificationEvent("Введите название домика", false);
                return;
            }

            if (NewCabinCapacity <= 0)
            {
                ShowNotificationEvent("Вместимость должна быть больше 0", false);
                return;
            }

            if (NewCabinPrice <= 0)
            {
                ShowNotificationEvent("Цена должна быть больше 0", false);
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
                        Capacity = NewCabinCapacity,
                        PricePerNight = NewCabinPrice,
                        ImageUrl = "",
                        IsAvailable = true,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    context.Cabins.Add(cabin);
                    context.SaveChanges();

                    ShowNotificationEvent($"✅ Домик '{cabin.Name}' добавлен", true);

                    // Очистка формы
                    NewCabinName = string.Empty;
                    NewCabinDescription = string.Empty;
                    NewCabinCapacity = 2;
                    NewCabinPrice = 1000;

                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                ShowNotificationEvent($"Ошибка: {ex.Message}", false);
            }
        }

        // ════════════════════════════════════════════════════════════
        // РЕДАКТИРОВАНИЕ ДОМИКА
        // ════════════════════════════════════════════════════════════

        private void EditCabin(Cabins cabin)
        {
            if (cabin == null) return;
            RequestEdit?.Invoke(cabin);
        }

        // ════════════════════════════════════════════════════════════
        // УДАЛЕНИЕ ДОМИКА
        // ════════════════════════════════════════════════════════════

        private void DeleteCabin(Cabins cabin)
        {
            if (cabin == null) return;

            // Проверка: есть ли активные бронирования?
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    int activeBookings = context.Bookings
                        .Count(b => b.CabinId == cabin.CabinId && b.Status == "active");

                    if (activeBookings > 0)
                    {
                        MessageBox.Show(
                            $"Нельзя удалить домик с активными бронированиями ({activeBookings})",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show(
                        $"Удалить домик '{cabin.Name}'?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

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
                ShowNotificationEvent($"Ошибка: {ex.Message}", false);
            }
        }

        // ════════════════════════════════════════════════════════════
        // ВСПОМОГАТЕЛЬНЫЕ
        // ════════════════════════════════════════════════════════════

        private void ShowNotificationEvent(string message, bool isSuccess)
        {
            ShowNotification?.Invoke(message, isSuccess);
        }
    }
}