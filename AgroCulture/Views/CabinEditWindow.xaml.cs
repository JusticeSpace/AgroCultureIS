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

        private ObservableCollection<Amenities> _selectedAmenities = new ObservableCollection<Amenities>();

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

                    TxtName.Text = cabin.Name;
                    TxtDescription.Text = cabin.Description ?? "";
                    TxtCapacity.Text = cabin.MaxGuests.ToString();
                    TxtPrice.Text = cabin.PricePerNight.ToString();
                    CmbStatus.SelectedIndex = cabin.IsActive ? 0 : 1;

                    TxtCabinInfo.Text = $"ID: {cabin.CabinId} • {cabin.Name}";

                    _selectedAmenities.Clear();
                    foreach (var amenity in cabin.CabinAmenities.Select(ca => ca.Amenities).Distinct())
                    {
                        _selectedAmenities.Add(amenity);
                    }

                    AmenitiesListBox.ItemsSource = _selectedAmenities;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
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

                    cabin.Name = TxtName.Text.Trim();
                    cabin.Description = TxtDescription.Text.Trim();
                    cabin.MaxGuests = int.Parse(TxtCapacity.Text);
                    cabin.PricePerNight = decimal.Parse(TxtPrice.Text);
                    cabin.IsActive = CmbStatus.SelectedIndex == 0;

                    var existing = context.CabinAmenities
                        .Where(ca => ca.CabinId == EditCabinId).ToList();

                    foreach (var item in existing)
                    {
                        context.CabinAmenities.Remove(item);
                    }

                    foreach (var amenity in _selectedAmenities)
                    {
                        context.CabinAmenities.Add(new CabinAmenities
                        {
                            CabinId = cabin.CabinId,
                            AmenityId = amenity.AmenityId
                        });
                    }

                    context.SaveChanges();

                    MessageBox.Show("Успешно сохранено!", "Успех",
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
            DialogResultSuccess = false;
            this.Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResultSuccess = false;
            this.Close();
        }
    }
}