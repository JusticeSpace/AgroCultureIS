using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AgroCulture.Views
{
    public partial class CabinEditWindow : Window
    {
        public int EditCabinId { get; set; }
        public bool DialogResultSuccess { get; private set; } = false;

        private ObservableCollection<AmenityViewModel> _selectedAmenities = new ObservableCollection<AmenityViewModel>();
        private ObservableCollection<AmenityViewModel> _availableAmenities = new ObservableCollection<AmenityViewModel>();

        // Вспомогательный класс для отображения удобств
        public class AmenityViewModel
        {
            public int AmenityId { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public string DisplayName => $"{Icon} {Name}";

            public AmenityViewModel(Amenities amenity)
            {
                AmenityId = amenity.AmenityId;
                Name = amenity.Name;
                Icon = amenity.Icon ?? "📌";
            }
        }

        public CabinEditWindow()
        {
            InitializeComponent();
        }

        public CabinEditWindow(int cabinId) : this()
        {
            EditCabinId = cabinId;
            Loaded += CabinEditWindow_Loaded;
        }

        private void CabinEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCabinData();
            LoadAvailableAmenities();
        }

        private void LoadCabinData()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var cabin = context.Cabins
                        .Include("CabinAmenities.Amenities")
                        .FirstOrDefault(c => c.CabinId == EditCabinId);

                    if (cabin == null)
                    {
                        MessageBox.Show("Домик не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        this.Close();
                        return;
                    }

                    // Загружаем основные данные
                    TxtName.Text = cabin.Name;
                    TxtDescription.Text = cabin.Description ?? "";
                    TxtCapacity.Text = cabin.MaxGuests.ToString();
                    TxtPrice.Text = cabin.PricePerNight.ToString();
                    CmbStatus.SelectedIndex = cabin.IsActive ? 0 : 1;

                    TxtCabinInfo.Text = $"ID: {cabin.CabinId} • {cabin.Name}";

                    // Загружаем выбранные удобства
                    _selectedAmenities.Clear();
                    foreach (var amenity in cabin.CabinAmenities.Select(ca => ca.Amenities).Distinct())
                    {
                        if (amenity != null)
                        {
                            _selectedAmenities.Add(new AmenityViewModel(amenity));
                        }
                    }

                    AmenitiesListBox.ItemsSource = _selectedAmenities;
                    UpdateAmenitiesVisibility();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void LoadAvailableAmenities()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // Загружаем все доступные удобства
                    var allAmenities = context.Amenities.OrderBy(a => a.Name).ToList();

                    _availableAmenities.Clear();
                    foreach (var amenity in allAmenities)
                    {
                        _availableAmenities.Add(new AmenityViewModel(amenity));
                    }

                    UpdateAvailableAmenitiesComboBox();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки удобств:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateAvailableAmenitiesComboBox()
        {
            // Фильтруем доступные удобства - показываем только те, которые еще не выбраны
            var selectedIds = _selectedAmenities.Select(a => a.AmenityId).ToList();
            var availableToAdd = _availableAmenities.Where(a => !selectedIds.Contains(a.AmenityId)).ToList();

            CmbAmenities.ItemsSource = availableToAdd;
            CmbAmenities.SelectedIndex = -1;

            // Деактивируем кнопку добавления если нет доступных удобств
            BtnAddAmenity.IsEnabled = availableToAdd.Any();

            if (!availableToAdd.Any())
            {
                CmbAmenities.Text = "Все удобства уже добавлены";
            }
        }

        private void UpdateAmenitiesVisibility()
        {
            // Обновляем видимость плейсхолдера
            if (_selectedAmenities.Any())
            {
                NoAmenitiesPlaceholder.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoAmenitiesPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void BtnAddAmenity_Click(object sender, RoutedEventArgs e)
        {
            if (CmbAmenities.SelectedItem is AmenityViewModel selectedAmenity)
            {
                // Добавляем удобство в список выбранных
                _selectedAmenities.Add(selectedAmenity);

                // Обновляем UI
                UpdateAvailableAmenitiesComboBox();
                UpdateAmenitiesVisibility();

                // Показываем уведомление
                var snackbar = new MaterialDesignThemes.Wpf.Snackbar
                {
                    Message = new MaterialDesignThemes.Wpf.SnackbarMessage
                    {
                        Content = $"✅ Добавлено: {selectedAmenity.Name}"
                    },
                    IsActive = true
                };

                // Анимация добавления (опционально)
                System.Windows.Media.Animation.DoubleAnimation fadeIn =
                    new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                AmenitiesListBox.BeginAnimation(OpacityProperty, fadeIn);
            }
            else
            {
                MessageBox.Show("Выберите удобство для добавления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRemoveAmenity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null && int.TryParse(button.Tag.ToString(), out int amenityId))
            {
                var amenity = _selectedAmenities.FirstOrDefault(a => a.AmenityId == amenityId);
                if (amenity != null)
                {
                    _selectedAmenities.Remove(amenity);

                    // Обновляем UI
                    UpdateAvailableAmenitiesComboBox();
                    UpdateAmenitiesVisibility();
                }
            }
        }

        private bool ValidateForm()
        {
            // ✅ Валидация названия через ValidationService
            var nameValidation = Services.ValidationService.ValidateCabinName(TxtName.Text);
            if (!nameValidation.isValid)
            {
                MessageBox.Show(nameValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return false;
            }

            // ✅ Парсинг и валидация вместимости
            var capacityParse = Services.ValidationService.TryParseInt(TxtCapacity.Text, "Вместимость");
            if (!capacityParse.success)
            {
                MessageBox.Show(capacityParse.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCapacity.Focus();
                return false;
            }

            var capacityValidation = Services.ValidationService.ValidateCabinCapacity(capacityParse.value);
            if (!capacityValidation.isValid)
            {
                MessageBox.Show(capacityValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCapacity.Focus();
                return false;
            }

            // ✅ Парсинг и валидация цены
            var priceParse = Services.ValidationService.TryParseDecimal(TxtPrice.Text, "Цена");
            if (!priceParse.success)
            {
                MessageBox.Show(priceParse.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPrice.Focus();
                return false;
            }

            var priceValidation = Services.ValidationService.ValidateCabinPrice(priceParse.value);
            if (!priceValidation.isValid)
            {
                MessageBox.Show(priceValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPrice.Focus();
                return false;
            }

            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var cabin = context.Cabins.FirstOrDefault(c => c.CabinId == EditCabinId);
                    if (cabin == null)
                    {
                        MessageBox.Show("Домик не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Обновляем основные данные
                    cabin.Name = TxtName.Text.Trim();
                    cabin.Description = TxtDescription.Text.Trim();
                    cabin.MaxGuests = int.Parse(TxtCapacity.Text);
                    cabin.PricePerNight = decimal.Parse(TxtPrice.Text);
                    cabin.IsActive = CmbStatus.SelectedIndex == 0;

                    // Удаляем старые связи с удобствами
                    var existing = context.CabinAmenities
                        .Where(ca => ca.CabinId == EditCabinId).ToList();

                    foreach (var item in existing)
                    {
                        context.CabinAmenities.Remove(item);
                    }

                    // Добавляем новые связи с удобствами
                    foreach (var amenity in _selectedAmenities)
                    {
                        context.CabinAmenities.Add(new CabinAmenities
                        {
                            CabinId = cabin.CabinId,
                            AmenityId = amenity.AmenityId
                        });
                    }

                    context.SaveChanges();

                    MessageBox.Show($"✅ Домик \"{cabin.Name}\" успешно обновлен!\n" +
                                  $"Добавлено удобств: {_selectedAmenities.Count}",
                                  "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResultSuccess = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите отменить изменения?",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DialogResultSuccess = false;
                this.Close();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            BtnCancel_Click(sender, e);
        }
    }
}