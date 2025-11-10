using System;
using System.Collections.Generic;
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
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigureUIByRole();
            LoadUserData();
            NavigateToDefaultPage();
        }

        // ═══════════════════════════════════════════════════════════
        // КОНФИГУРАЦИЯ UI ПО РОЛИ
        // ═══════════════════════════════════════════════════════════

        private void ConfigureUIByRole()
        {
            if (App.CurrentUser == null)
            {
                ShowLoginAndClose();
                return;
            }

            string role = App.CurrentUser.Role.ToLower();

            switch (role)
            {
                case "admin":
                    // Админ видит всё
                    StaffTabButton.Visibility = Visibility.Visible;
                    BookingTabButton.Visibility = Visibility.Visible;
                    ListTabButton.Visibility = Visibility.Visible;
                    ProfileButton.Visibility = Visibility.Visible;
                    UserNameCard.Visibility = Visibility.Visible;  // ✅ Показываем карточку

                    // Названия вкладок
                    SetBookingTabText("Бронирование");
                    break;

                case "manager":
                    // Менеджер видит бронирование и список
                    StaffTabButton.Visibility = Visibility.Collapsed;
                    BookingTabButton.Visibility = Visibility.Visible;
                    ListTabButton.Visibility = Visibility.Visible;
                    ProfileButton.Visibility = Visibility.Visible;
                    UserNameCard.Visibility = Visibility.Visible;  // ✅ Показываем карточку

                    SetBookingTabText("Бронирование");
                    break;

                case "guest":
                    // ✅ ИСПРАВЛЕНО: Гость видит только каталог домиков
                    StaffTabButton.Visibility = Visibility.Collapsed;
                    BookingTabButton.Visibility = Visibility.Visible;  // ✅ ПОКАЗЫВАЕМ
                    ListTabButton.Visibility = Visibility.Collapsed;   // ✅ СКРЫВАЕМ
                    ProfileButton.Visibility = Visibility.Collapsed;   // ✅ Скрываем
                    UserNameCard.Visibility = Visibility.Collapsed;    // ✅ СКРЫВАЕМ карточку

                    // ✅ Меняем название вкладки
                    SetBookingTabText("Каталог домиков");
                    break;

                default:
                    StaffTabButton.Visibility = Visibility.Collapsed;
                    BookingTabButton.Visibility = Visibility.Collapsed;
                    ListTabButton.Visibility = Visibility.Visible;
                    ProfileButton.Visibility = Visibility.Collapsed;
                    UserNameCard.Visibility = Visibility.Collapsed;

                    SetBookingTabText("Бронирование");
                    break;
            }

            ConfigureNavigationGrid();
        }

        /// <summary>
        /// ✅ НОВЫЙ МЕТОД: Изменение текста вкладки бронирования
        /// </summary>
        private void SetBookingTabText(string text)
        {
            var stack = BookingTabButton.Content as StackPanel;
            if (stack != null)
            {
                foreach (var child in stack.Children)
                {
                    if (child is TextBlock textBlock)
                    {
                        textBlock.Text = text;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Настройка Grid колонок и позиций кнопок
        /// </summary>
        private void ConfigureNavigationGrid()
        {
            NavigationTabsGrid.ColumnDefinitions.Clear();

            var visibleButtons = new List<Button>();

            if (StaffTabButton.Visibility == Visibility.Visible)
                visibleButtons.Add(StaffTabButton);

            if (BookingTabButton.Visibility == Visibility.Visible)
                visibleButtons.Add(BookingTabButton);

            if (ListTabButton.Visibility == Visibility.Visible)
                visibleButtons.Add(ListTabButton);

            if (visibleButtons.Count == 0) return;

            // Создание равных колонок
            for (int i = 0; i < visibleButtons.Count; i++)
            {
                NavigationTabsGrid.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                );
            }

            // Расстановка кнопок по колонкам
            for (int i = 0; i < visibleButtons.Count; i++)
            {
                Grid.SetColumn(visibleButtons[i], i);

                if (visibleButtons.Count == 1)
                {
                    visibleButtons[i].Margin = new Thickness(0);
                }
                else if (i == 0)
                {
                    visibleButtons[i].Margin = new Thickness(0, 0, 4, 0);
                }
                else if (i == visibleButtons.Count - 1)
                {
                    visibleButtons[i].Margin = new Thickness(4, 0, 0, 0);
                }
                else
                {
                    visibleButtons[i].Margin = new Thickness(4, 0, 4, 0);
                }
            }

            System.Diagnostics.Debug.WriteLine($"[NAV] Настроено {visibleButtons.Count} табов для роли: {App.CurrentUser?.Role}");
        }

        // ═══════════════════════════════════════════════════════════
        // ПУБЛИЧНЫЙ МЕТОД ДЛЯ ОБНОВЛЕНИЯ HEADER
        // ═══════════════════════════════════════════════════════════

        public void UpdateUserDisplay()
        {
            if (App.CurrentUser != null)
            {
                UserNameText.Text = App.CurrentUser.FullName;
                UserRoleText.Text = GetRoleDisplayName(App.CurrentUser.Role);

                Color roleBgColor;
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

                UserRoleBadge.Background = new SolidColorBrush(roleBgColor);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // НАВИГАЦИЯ НА СТАРТОВУЮ СТРАНИЦУ
        // ═══════════════════════════════════════════════════════════

        private void NavigateToDefaultPage()
        {
            string role = App.CurrentUser.Role.ToLower();

            System.Diagnostics.Debug.WriteLine($"[NAV] Загрузка стартовой страницы для роли: {role}");

            switch (role)
            {
                case "admin":
                    NavigateToStaff();
                    SetActiveTab(StaffTabButton);
                    System.Diagnostics.Debug.WriteLine("[NAV] → Страница сотрудников");
                    break;

                case "manager":
                    NavigateToBooking();
                    SetActiveTab(BookingTabButton);
                    System.Diagnostics.Debug.WriteLine("[NAV] → Страница бронирования");
                    break;

                case "guest":
                    // ✅ ИСПРАВЛЕНО: Гость попадает на каталог домиков
                    NavigateToBooking();
                    SetActiveTab(BookingTabButton);
                    System.Diagnostics.Debug.WriteLine("[NAV] → Каталог домиков (режим гостя)");
                    break;

                default:
                    NavigateToList();
                    SetActiveTab(ListTabButton);
                    System.Diagnostics.Debug.WriteLine("[NAV] → Список бронирований (fallback)");
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ЗАГРУЗКА ДАННЫХ ПОЛЬЗОВАТЕЛЯ
        // ═══════════════════════════════════════════════════════════

        private void LoadUserData()
        {
            if (App.CurrentUser == null)
            {
                ShowLoginAndClose();
                return;
            }

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
        }

        private string GetRoleDisplayName(string role)
        {
            switch (role?.ToLower())
            {
                case "admin": return "Администратор";
                case "manager": return "Менеджер";
                case "guest": return "Гость";
                default: return "Пользователь";
            }
        }

        private void ShowLoginAndClose()
        {
            MessageBox.Show("Ошибка авторизации. Войдите снова.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);

            App.CurrentUser = null;
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        // ═══════════════════════════════════════════════════════════
        // НАВИГАЦИЯ ПО ТАБАМ
        // ═══════════════════════════════════════════════════════════

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

        // ═══════════════════════════════════════════════════════════
        // МЕТОДЫ НАВИГАЦИИ
        // ═══════════════════════════════════════════════════════════

        private void NavigateToStaff()
        {
            MainContentFrame.Content = new StaffManagementView();
        }

        private void NavigateToBooking()
        {
            MainContentFrame.Content = new CabinBookingView();
        }

        private void NavigateToList()
        {
            MainContentFrame.Content = new BookingsListView();
        }

        // ═══════════════════════════════════════════════════════════
        // ВИЗУАЛЬНЫЕ ЭФФЕКТЫ ТАБОВ
        // ═══════════════════════════════════════════════════════════

        private void SetActiveTab(Button activeButton)
        {
            if (StaffTabButton.Visibility == Visibility.Visible)
                ResetTabButton(StaffTabButton);

            if (BookingTabButton.Visibility == Visibility.Visible)
                ResetTabButton(BookingTabButton);

            if (ListTabButton.Visibility == Visibility.Visible)
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

        // ═══════════════════════════════════════════════════════════
        // КНОПКИ HEADER
        // ═══════════════════════════════════════════════════════════

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Content = new ProfilePage();

            ResetTabButton(StaffTabButton);
            ResetTabButton(BookingTabButton);
            ResetTabButton(ListTabButton);
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