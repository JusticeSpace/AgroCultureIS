using System;
using System.Windows;
using System.Windows.Controls;
using AgroCulture.ViewModels;

namespace AgroCulture.Views
{
    public partial class CabinsManagementView : UserControl
    {
        private CabinsManagementViewModel ViewModel => DataContext as CabinsManagementViewModel;

        public CabinsManagementView()
        {
            InitializeComponent();
            Loaded += CabinsManagementView_Loaded;
        }

        private void CabinsManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ShowNotification -= ShowNotification;
                ViewModel.ShowNotification += ShowNotification;

                ViewModel.RequestEdit -= OpenEditDialog;
                ViewModel.RequestEdit += OpenEditDialog;

                ViewModel.RefreshData();
            }
        }

        private void ShowNotification(string message, bool isSuccess)
        {
            MessageBox.Show(message,
                isSuccess ? "Успех" : "Ошибка",
                MessageBoxButton.OK,
                isSuccess ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }

        private void OpenEditDialog(Cabins cabin)
        {
            if (cabin == null) return;

            try
            {
                var window = new CabinEditWindow(cabin.CabinId);
                window.Owner = Window.GetWindow(this);
                window.ShowDialog();

                if (window.DialogResultSuccess && ViewModel != null)
                {
                    ViewModel.RefreshData();
                    ShowNotification("✅ Изменения сохранены!", true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}