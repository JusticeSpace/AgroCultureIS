using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AgroCulture.Views;

namespace AgroCulture
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ✅ ОТЛОЖЕННАЯ НАВИГАЦИЯ (после загрузки окна)
            Loaded += MainWindow_Loaded;
        }

        // ✅ ВЫЗЫВАЕТСЯ ПОСЛЕ ПОЛНОЙ ЗАГРУЗКИ ОКНА
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserData();

            // ✅ ТЕПЕРЬ НАВИГАЦИЯ ПРОИСХОДИТ КОГДА ВСЁ ГОТОВО
            if (App.CurrentUser != null && App.CurrentUser.Role.ToLower() == "guest")
            {
                NavigateToBooking();
                SetActiveTab(BookingTabButton);
            }
            else
            {
                NavigateToStaff();
                SetActiveTab(StaffTabButton);
            }
        }

        // ========================================
        // ЗАГРУЗКА ДАННЫХ ПОЛЬЗОВАТЕЛЯ
        // ========================================

        private void LoadUserData()
        {
            if (App.CurrentUser != null)
            {
                UserNameText.Text = App.CurrentUser.FullName;

                string roleText = GetRoleDisplayName(App.CurrentUser.Role);
                Color roleBgColor;
                Color roleFgColor = Colors.White;

                switch (App.CurrentUser.Role.ToLower())
                {
                    case "admin":
                        roleBgColor = (Color)ColorConverter.ConvertFromString("#7c3aed");
                        break;
                    case "manager":
                        roleBgColor = (Color)ColorConverter.ConvertFromString("#2563eb");
                        break;
                    case "guest":
                        roleBgColor = (Color)ColorConverter.ConvertFromString("#6b7280");
                        break;
                    default:
                        roleBgColor = Colors.Gray;
                        break;
                }

                UserRoleText.Text = roleText;
                UserRoleBadge.Background = new SolidColorBrush(roleBgColor);
                UserRoleText.Foreground = new SolidColorBrush(roleFgColor);

                // Скрываем вкладку "Сотрудники" для гостей
                if (App.CurrentUser.Role.ToLower() == "guest")
                {
                    StaffTabButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageBox.Show("Ошибка авторизации. Войдите снова.", "Ошибка");
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private string GetRoleDisplayName(string role)
        {
            switch (role?.ToLower())
            {
                case "admin":
                    return "Администратор";
                case "manager":
                    return "Менеджер";
                case "guest":
                    return "Гость";
                default:
                    return "Пользователь";
            }
        }

        // ========================================
        // НАВИГАЦИЯ ПО ТАБАМ
        // ========================================

        private void StaffTabButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToStaff();
            SetActiveTab(StaffTabButton);
        }

        private void BookingTabButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToBooking();
            SetActiveTab(BookingTabButton);
        }

        private void ListTabButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToList();
            SetActiveTab(ListTabButton);
        }

        // ========================================
        // МЕТОДЫ НАВИГАЦИИ
        // ========================================

        private void NavigateToStaff()
        {
            MainContentFrame.Content = new StaffManagementView();
        }

        private void NavigateToBooking()
        {
            var textBlock = new TextBlock
            {
                Text = "🏠 Бронирование домиков\n\n(Страница в разработке)",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6b7280")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            MainContentFrame.Content = textBlock;
        }

        private void NavigateToList()
        {
            var textBlock = new TextBlock
            {
                Text = "📋 Список бронирований\n\n(Страница в разработке)",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6b7280")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            MainContentFrame.Content = textBlock;
        }

        // ========================================
        // ВИЗУАЛЬНЫЕ ЭФФЕКТЫ ТАБОВ
        // ========================================

        private void SetActiveTab(Button activeButton)
        {
            ResetTabButton(StaffTabButton);
            ResetTabButton(BookingTabButton);
            ResetTabButton(ListTabButton);
            ActivateTabButton(activeButton);
        }

        private void ResetTabButton(Button button)
        {
            button.Background = Brushes.Transparent;

            var stack = button.Content as StackPanel;
            if (stack != null)
            {
                foreach (var child in stack.Children)
                {
                    if (child is MaterialDesignThemes.Wpf.PackIcon icon)
                        icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6b7280"));
                    if (child is TextBlock text)
                        text.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6b7280"));
                }
            }
        }

        private void ActivateTabButton(Button button)
        {
            Color backgroundColor;
            Color foregroundColor;

            if (button == StaffTabButton)
            {
                backgroundColor = (Color)ColorConverter.ConvertFromString("#F3E8FF");
                foregroundColor = (Color)ColorConverter.ConvertFromString("#7c3aed");
            }
            else if (button == BookingTabButton)
            {
                backgroundColor = (Color)ColorConverter.ConvertFromString("#DBEAFE");
                foregroundColor = (Color)ColorConverter.ConvertFromString("#2563eb");
            }
            else if (button == ListTabButton)
            {
                backgroundColor = (Color)ColorConverter.ConvertFromString("#DCFCE7");
                foregroundColor = (Color)ColorConverter.ConvertFromString("#16a34a");
            }
            else
            {
                backgroundColor = (Color)ColorConverter.ConvertFromString("#15803d");
                foregroundColor = Colors.White;
            }

            button.Background = new SolidColorBrush(backgroundColor);

            var stack = button.Content as StackPanel;
            if (stack != null)
            {
                foreach (var child in stack.Children)
                {
                    if (child is MaterialDesignThemes.Wpf.PackIcon icon)
                        icon.Foreground = new SolidColorBrush(foregroundColor);
                    if (child is TextBlock text)
                        text.Foreground = new SolidColorBrush(foregroundColor);
                }
            }
        }

        // ========================================
        // КНОПКИ HEADER
        // ========================================

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
            {
                MessageBox.Show(
                    $"ФИО: {App.CurrentUser.FullName}\n" +
                    $"Логин: {App.CurrentUser.Username}\n" +
                    $"Роль: {GetRoleDisplayName(App.CurrentUser.Role)}\n" +
                    $"Телефон: {App.CurrentUser.Phone ?? "Не указан"}\n" +
                    $"Email: {App.CurrentUser.Email ?? "Не указан"}",
                    "Профиль пользователя",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы действительно хотите выйти из системы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                App.CurrentUser = null;
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}