using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace AgroCulture.Views
{
    public partial class ProfilePage : UserControl
    {
        private Users _currentUser;

        public ProfilePage()
        {
            InitializeComponent();
            Loaded += ProfilePage_Loaded;
        }

        private void ProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            _currentUser = App.CurrentUser;

            if (_currentUser == null)
            {
                MessageBox.Show("Ошибка: пользователь не авторизован", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadUserData();
            UpdateRoleBadge(_currentUser.Role);
            InitializeStatistics();
        }

        // ═══════════════════════════════════════════════════════════
        // ЗАГРУЗКА ДАННЫХ ПОЛЬЗОВАТЕЛЯ
        // ═══════════════════════════════════════════════════════════

        private void LoadUserData()
        {
            TxtUserFullName.Text = _currentUser.FullName;
            TxtFullNameView.Text = _currentUser.FullName;
            TxtUsername.Text = _currentUser.Username;
            TxtRoleDisplay.Text = GetRoleDisplayName(_currentUser.Role);

            // ✅ Телефон
            TxtPhone.Text = string.IsNullOrWhiteSpace(_currentUser.Phone)
                ? "Не указан"
                : _currentUser.Phone;

            // ✅ Email
            TxtEmail.Text = string.IsNullOrWhiteSpace(_currentUser.Email)
                ? "Не указан"
                : _currentUser.Email;

            // ✅ Дата регистрации
            TxtCreatedAt.Text = _currentUser.CreatedAt.ToString("dd MMMM yyyy");

            // Первая буква для аватара
            TxtAvatarInitial.Text = string.IsNullOrWhiteSpace(_currentUser.FullName)
                ? "?"
                : _currentUser.FullName.Substring(0, 1).ToUpper();
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

        // ═══════════════════════════════════════════════════════════
        // НАСТРОЙКА BADGE РОЛИ
        // ═══════════════════════════════════════════════════════════

        private void UpdateRoleBadge(string role)
        {
            Color bgColor, borderColor, textColor;
            PackIconKind iconKind;
            string description;

            switch (role?.ToLower())
            {
                case "admin":
                    bgColor = Color.FromRgb(243, 232, 255);    // Purple-100
                    borderColor = Color.FromRgb(216, 180, 254); // Purple-300
                    textColor = Color.FromRgb(126, 34, 206);    // Purple-700
                    iconKind = PackIconKind.Shield;
                    description = "Полный доступ к системе: управление сотрудниками, бронирования, статистика";
                    break;

                case "manager":
                    bgColor = Color.FromRgb(219, 234, 254);    // Blue-100
                    borderColor = Color.FromRgb(147, 197, 253); // Blue-300
                    textColor = Color.FromRgb(29, 78, 216);     // Blue-700
                    iconKind = PackIconKind.Account;
                    description = "Создание и редактирование бронирований, просмотр списка бронирований";
                    break;

                case "guest":
                    bgColor = Color.FromRgb(243, 244, 246);    // Gray-100
                    borderColor = Color.FromRgb(229, 231, 235); // Gray-300
                    textColor = Color.FromRgb(55, 65, 81);      // Gray-700
                    iconKind = PackIconKind.Account;
                    description = "Только просмотр списка бронирований";
                    break;

                default:
                    bgColor = Colors.LightGray;
                    borderColor = Colors.Gray;
                    textColor = Colors.Black;
                    iconKind = PackIconKind.Account;
                    description = "";
                    break;
            }

            BorderRoleBadge.Background = new SolidColorBrush(bgColor);
            BorderRoleBadge.BorderBrush = new SolidColorBrush(borderColor);
            TxtRoleName.Foreground = new SolidColorBrush(textColor);
            IconRole.Foreground = new SolidColorBrush(textColor);
            IconRole.Kind = iconKind;
            TxtRoleDescription.Text = description;
        }

        // ═══════════════════════════════════════════════════════════
        // РЕДАКТИРОВАНИЕ ПРОФИЛЯ
        // ═══════════════════════════════════════════════════════════

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            TxtFullNameView.Visibility = Visibility.Collapsed;
            PanelEditFullName.Visibility = Visibility.Visible;
            BtnEditProfile.Visibility = Visibility.Collapsed;

            TxtFullNameEdit.Text = TxtFullNameView.Text;
            TxtFullNameEdit.Focus();
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            string newFullName = TxtFullNameEdit.Text.Trim();

            if (string.IsNullOrWhiteSpace(newFullName))
            {
                MessageBox.Show("ФИО не может быть пустым", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);
                    if (user != null)
                    {
                        user.FullName = newFullName;
                        context.SaveChanges();

                        // Обновление локальных данных
                        _currentUser.FullName = newFullName;
                        App.CurrentUser.FullName = newFullName;

                        TxtFullNameView.Text = newFullName;
                        TxtUserFullName.Text = newFullName;
                        TxtAvatarInitial.Text = newFullName.Substring(0, 1).ToUpper();

                        TxtFullNameView.Visibility = Visibility.Visible;
                        PanelEditFullName.Visibility = Visibility.Collapsed;
                        BtnEditProfile.Visibility = Visibility.Visible;

                        // ✅ НОВОЕ: Обновляем header в MainWindow
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow?.UpdateUserDisplay();

                        MessageBox.Show("Профиль успешно обновлен", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления профиля:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelEditProfile_Click(object sender, RoutedEventArgs e)
        {
            TxtFullNameView.Visibility = Visibility.Visible;
            PanelEditFullName.Visibility = Visibility.Collapsed;
            BtnEditProfile.Visibility = Visibility.Visible;
        }

        // ═══════════════════════════════════════════════════════════
        // СМЕНА ПАРОЛЯ
        // ═══════════════════════════════════════════════════════════

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            BtnChangePassword.Visibility = Visibility.Collapsed;
            PanelPasswordChange.Visibility = Visibility.Visible;
        }

        private void BtnSavePassword_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = TxtCurrentPassword.Password;
            string newPassword = TxtNewPassword.Password;
            string confirmPassword = TxtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                MessageBox.Show("Введите текущий пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword.Length < 4)
            {
                MessageBox.Show("Новый пароль должен содержать минимум 4 символа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);

                    if (user == null)
                    {
                        MessageBox.Show("Пользователь не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Проверка текущего пароля
                    if (user.PasswordHash != currentPassword)
                    {
                        MessageBox.Show("Неверный текущий пароль", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Обновление пароля
                    user.PasswordHash = newPassword;
                    context.SaveChanges();

                    TxtCurrentPassword.Clear();
                    TxtNewPassword.Clear();
                    TxtConfirmPassword.Clear();

                    BtnChangePassword.Visibility = Visibility.Visible;
                    PanelPasswordChange.Visibility = Visibility.Collapsed;

                    MessageBox.Show("Пароль успешно изменен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка смены пароля:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelPassword_Click(object sender, RoutedEventArgs e)
        {
            TxtCurrentPassword.Clear();
            TxtNewPassword.Clear();
            TxtConfirmPassword.Clear();

            BtnChangePassword.Visibility = Visibility.Visible;
            PanelPasswordChange.Visibility = Visibility.Collapsed;
        }

        // ═══════════════════════════════════════════════════════════
        // АДАПТИВНАЯ СТАТИСТИКА
        // ═══════════════════════════════════════════════════════════

        private void InitializeStatistics()
        {
            GridStatistics.Children.Clear();
            GridStatistics.ColumnDefinitions.Clear();

            var stats = GetStatisticsByRole(_currentUser.Role);

            if (stats.Count == 0) return;

            // Создание колонок
            for (int i = 0; i < stats.Count; i++)
            {
                GridStatistics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                if (i < stats.Count - 1)
                {
                    GridStatistics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
                }
            }

            // Создание карточек
            int colIndex = 0;
            foreach (var stat in stats)
            {
                var card = CreateStatCard(stat);
                Grid.SetColumn(card, colIndex);
                GridStatistics.Children.Add(card);

                colIndex += 2;
            }
        }

        private Border CreateStatCard(StatisticItem stat)
        {
            var border = new Border
            {
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(249, 250, 251), 0),
                        new GradientStop(Colors.White, 1)
                    }
                },
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16)
            };

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            // Иконка
            var iconBorder = new Border
            {
                Width = 48,
                Height = 48,
                Background = new SolidColorBrush(Color.FromRgb(243, 244, 246)),
                CornerRadius = new CornerRadius(24),
                Child = new PackIcon
                {
                    Kind = stat.IconKind,
                    Width = 20,
                    Height = 20,
                    Foreground = new SolidColorBrush(stat.Color),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            stackPanel.Children.Add(iconBorder);

            // Текст
            var textPanel = new StackPanel
            {
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            textPanel.Children.Add(new TextBlock
            {
                Text = stat.Label,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                TextTrimming = TextTrimming.CharacterEllipsis
            });

            textPanel.Children.Add(new TextBlock
            {
                Text = stat.Value,
                FontSize = 24,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(stat.Color)
            });

            stackPanel.Children.Add(textPanel);
            border.Child = stackPanel;

            return border;
        }

        private List<StatisticItem> GetStatisticsByRole(string role)
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    switch (role?.ToLower())
                    {
                        case "admin":
                            // ✅ ИСПРАВЛЕНО: убрали IsActive из запроса
                            int totalStaff = context.Users
                                .Count(u => u.Role == "admin" || u.Role == "manager");

                            int totalBookings = context.Bookings.Count();

                            // ✅ ИСПРАВЛЕНО: просто количество домиков
                            int totalCabins = context.Cabins.Count();

                            return new List<StatisticItem>
                    {
                        new StatisticItem
                        {
                            Label = "Всего сотрудников",
                            Value = totalStaff.ToString(),
                            IconKind = PackIconKind.Account,
                            Color = Color.FromRgb(147, 51, 234) // Purple
                        },
                        new StatisticItem
                        {
                            Label = "Всего бронирований",
                            Value = totalBookings.ToString(),
                            IconKind = PackIconKind.CalendarMonth,
                            Color = Color.FromRgb(37, 99, 235) // Blue
                        },
                        new StatisticItem
                        {
                            Label = "Всего домиков",
                            Value = totalCabins.ToString(),
                            IconKind = PackIconKind.Home,
                            Color = Color.FromRgb(22, 163, 74) // Green
                        }
                    };

                        case "manager":
                            // ✅ ИСПРАВЛЕНО: используем CreatedBy вместо ManagerId
                            int managerBookings = context.Bookings
                                .Count(b => b.CreatedBy == _currentUser.UserId);

                            int thisMonthBookings = context.Bookings
                                .Count(b =>
                                    b.CreatedBy == _currentUser.UserId &&
                                    b.CreatedAt.Month == DateTime.Now.Month &&
                                    b.CreatedAt.Year == DateTime.Now.Year);

                            return new List<StatisticItem>
                    {
                        new StatisticItem
                        {
                            Label = "Созданных бронирований",
                            Value = managerBookings.ToString(),
                            IconKind = PackIconKind.CalendarMonth,
                            Color = Color.FromRgb(37, 99, 235) // Blue
                        },
                        new StatisticItem
                        {
                            Label = "В этом месяце",
                            Value = thisMonthBookings.ToString(),
                            IconKind = PackIconKind.ChartBar,
                            Color = Color.FromRgb(22, 163, 74) // Green
                        }
                    };

                        case "guest":
                            int totalAvailableBookings = context.Bookings.Count();

                            return new List<StatisticItem>
                    {
                        new StatisticItem
                        {
                            Label = "Доступно бронирований",
                            Value = totalAvailableBookings.ToString(),
                            IconKind = PackIconKind.CalendarMonth,
                            Color = Color.FromRgb(107, 114, 128) // Gray
                        }
                    };

                        default:
                            return new List<StatisticItem>();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки статистики: {ex.Message}");

                // Возвращаем пустую статистику при ошибке
                return new List<StatisticItem>
        {
            new StatisticItem
            {
                Label = "Статистика недоступна",
                Value = "—",
                IconKind = PackIconKind.AlertCircle,
                Color = Color.FromRgb(239, 68, 68) // Red
            }
        };
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ВСПОМОГАТЕЛЬНЫЙ КЛАСС
        // ═══════════════════════════════════════════════════════════

        private class StatisticItem
        {
            public string Label { get; set; }
            public string Value { get; set; }
            public PackIconKind IconKind { get; set; }
            public Color Color { get; set; }
        }
    }
}