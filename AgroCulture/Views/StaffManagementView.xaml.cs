using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AgroCulture.ViewModels;
using MaterialDesignThemes.Wpf;

namespace AgroCulture.Views
{
    public partial class StaffManagementView : UserControl
    {
        private StaffViewModel ViewModel => DataContext as StaffViewModel;
        private bool _isEditWindowOpen = false; // ✅ ФЛАГ ДЛЯ ЗАЩИТЫ ОТ ПОВТОРНОГО ОТКРЫТИЯ

        public StaffManagementView()
        {
            InitializeComponent();

            System.Diagnostics.Debug.WriteLine("[STAFF VIEW] Конструктор вызван");

            DataContext = StaffViewModel.Instance;
            ViewModel?.RefreshData();

            Loaded += StaffManagementView_Loaded;
            Unloaded += StaffManagementView_Unloaded;
        }

        // ═══════════════════════════════════════════════════════════
        // СОБЫТИЯ ЖИЗНЕННОГО ЦИКЛА
        // ═══════════════════════════════════════════════════════════

        private void StaffManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[STAFF VIEW] Loaded вызван");

            if (ViewModel != null)
            {
                ViewModel.ShowNotification -= ShowNotification;
                ViewModel.ShowNotification += ShowNotification;

                ViewModel.RequestEdit -= OpenEditDialog;
                ViewModel.RequestEdit += OpenEditDialog;

                ViewModel.RefreshData();

                System.Diagnostics.Debug.WriteLine($"[STAFF VIEW] Загружено {ViewModel.StaffList?.Count ?? 0} сотрудников");
            }
        }

        private void StaffManagementView_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[STAFF VIEW] Unloaded - отписка от событий");

            // ✅ ОТПИСКА ОТ СОБЫТИЙ (предотвращение утечки памяти)
            if (ViewModel != null)
            {
                ViewModel.ShowNotification -= ShowNotification;
                ViewModel.RequestEdit -= OpenEditDialog;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ОБРАБОТЧИКИ СОБЫТИЙ
        // ═══════════════════════════════════════════════════════════

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.NewPassword = PasswordBox.Password;
            }
        }

        private void ShowNotification(string message, bool isSuccess)
        {
            try
            {
                var messageQueue = NotificationSnackbar.MessageQueue ?? new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
                NotificationSnackbar.MessageQueue = messageQueue;
                messageQueue.Enqueue(message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STAFF VIEW] Ошибка Snackbar: {ex.Message}");

                // Fallback на MessageBox
                MessageBox.Show(message,
                    isSuccess ? "Успех" : "Уведомление",
                    MessageBoxButton.OK,
                    isSuccess ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
        }

        private void OpenEditDialog(Users user)
        {
            // ✅ ЗАЩИТА ОТ ПОВТОРНОГО ОТКРЫТИЯ
            if (_isEditWindowOpen)
            {
                System.Diagnostics.Debug.WriteLine("[STAFF VIEW] Окно редактирования уже открыто!");
                return;
            }

            // ✅ ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА (на случай если флаг сбросился)
            if (Application.Current.Windows.OfType<StaffEditWindow>().Any())
            {
                System.Diagnostics.Debug.WriteLine("[STAFF VIEW] Окно StaffEditWindow найдено в списке окон!");
                return;
            }

            try
            {
                _isEditWindowOpen = true;
                System.Diagnostics.Debug.WriteLine($"[STAFF VIEW] Открытие окна редактирования для User ID: {user.UserId}");

                var window = new StaffEditWindow(user.UserId);
                window.Owner = Window.GetWindow(this);

                // ✅ ВАЖНО: Подписываемся на событие закрытия
                window.Closed += (s, e) =>
                {
                    _isEditWindowOpen = false;
                    System.Diagnostics.Debug.WriteLine("[STAFF VIEW] Окно редактирования закрыто");
                };

                window.ShowDialog();

                // ✅ Обновляем данные ТОЛЬКО если изменения сохранены
                if (window.DialogResultSuccess && ViewModel != null)
                {
                    System.Diagnostics.Debug.WriteLine("[STAFF VIEW] Изменения сохранены - обновление списка");
                    ViewModel.RefreshData();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[STAFF VIEW] Изменения отменены");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[STAFF VIEW] ОШИБКА: {ex.Message}");
                MessageBox.Show($"Ошибка открытия окна:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // ✅ ГАРАНТИРОВАННЫЙ СБРОС ФЛАГА
                _isEditWindowOpen = false;
            }
        }
    }
}