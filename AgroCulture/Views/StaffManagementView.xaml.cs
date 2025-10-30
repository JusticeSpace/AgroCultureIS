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

                ViewModel.RequestEdit -= OpenEditDialog;
                ViewModel.RequestEdit += OpenEditDialog;

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
            var editedUser = new Users
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role
            };

            var dialog = new Window
            {
                Title = "Редактировать сотрудника",
                Width = 500,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.SingleBorderWindow,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };

            var grid = new Grid { Margin = new Thickness(24) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var roleCombo = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 16),
                Height = 48
            };
            roleCombo.Items.Add(new ComboBoxItem { Content = "Менеджер", Tag = "manager" });
            roleCombo.Items.Add(new ComboBoxItem { Content = "Администратор", Tag = "admin" });
            roleCombo.SelectedIndex = editedUser.Role == "admin" ? 1 : 0;
            Grid.SetRow(roleCombo, 0);

            var loginBox = new TextBox
            {
                Text = editedUser.Username,
                Margin = new Thickness(0, 0, 0, 16),
                Height = 48
            };
            Grid.SetRow(loginBox, 1);

            var nameBox = new TextBox
            {
                Text = editedUser.FullName,
                Margin = new Thickness(0, 0, 0, 16),
                Height = 48
            };
            Grid.SetRow(nameBox, 2);

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 24, 0, 0)
            };
            Grid.SetRow(buttonsPanel, 4);

            var cancelButton = new Button
            {
                Content = "Отмена",
                Width = 120,
                Height = 40,
                Margin = new Thickness(0, 0, 16, 0)
            };
            cancelButton.Click += (s, evt) => dialog.Close();

            var saveButton = new Button
            {
                Content = "Сохранить",
                Width = 120,
                Height = 40,
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(46, 125, 50)),
                Foreground = System.Windows.Media.Brushes.White
            };
            saveButton.Click += (s, evt) =>
            {
                editedUser.FullName = nameBox.Text;
                editedUser.Username = loginBox.Text;
                editedUser.Role = ((ComboBoxItem)roleCombo.SelectedItem).Tag.ToString();

                ViewModel?.SaveStaffChanges(editedUser);
                dialog.Close();
            };

            buttonsPanel.Children.Add(cancelButton);
            buttonsPanel.Children.Add(saveButton);

            grid.Children.Add(roleCombo);
            grid.Children.Add(loginBox);
            grid.Children.Add(nameBox);
            grid.Children.Add(buttonsPanel);

            dialog.Content = grid;
            dialog.ShowDialog();
        }
    }
}