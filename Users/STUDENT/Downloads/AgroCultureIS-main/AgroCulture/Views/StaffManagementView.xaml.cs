using System;
using System.Windows;
using System.Windows.Controls;
using AgroCulture.ViewModels;
using MaterialDesignThemes.Wpf;

namespace AgroCulture.Views
{
    public partial class StaffManagementView : UserControl
    {
        private StaffViewModel ViewModel => DataContext as StaffViewModel;

        public StaffManagementView()
        {
            InitializeComponent();

            // ✅ УСТАНАВЛИВАЕМ SINGLETON КАК DataContext
            DataContext = StaffViewModel.Instance;

            // ✅ ПОДПИСКА НА СОБЫТИЕ ЗАГРУЗКИ
            Loaded += StaffManagementView_Loaded;
        }

        private void StaffManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                // ✅ БЕЗОПАСНАЯ ПОДПИСКА НА СОБЫТИЯ (избегаем дубликатов)
                ViewModel.ShowNotification -= ShowNotification;
                ViewModel.ShowNotification += ShowNotification;

                // ✅ ОБНОВЛЯЕМ ДАННЫЕ при каждом открытии страницы
                ViewModel.RefreshData();

                // ✅ ПРИНУДИТЕЛЬНОЕ ОБНОВЛЕНИЕ DataGrid (решает проблему первой загрузки)
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    StaffDataGrid.Items.Refresh();
                    StaffDataGrid.UpdateLayout();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.NewPassword = PasswordBox.Password;
            }
        }

        private void ShowNotification(string message, bool isSuccess)
        {
            var messageQueue = NotificationSnackbar.MessageQueue ?? new SnackbarMessageQueue();
            messageQueue.Enqueue(message);
        }

        private void OpenEditDialog(Users user)
        {
            try
            {
                var window = new Views.StaffEditWindow(user.UserId);
                window.Owner = Window.GetWindow(this);
                window.ShowDialog();

                if (window.DialogResultSuccess && ViewModel != null)
                {
                    ViewModel.LoadStaff();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия окна:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}