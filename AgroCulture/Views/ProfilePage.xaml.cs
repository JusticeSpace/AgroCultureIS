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
            // Основная информация
            TxtUserFullName.Text = _currentUser.FullName;

            // ✅ Заполняем 3 поля просмотра
            TxtSurnameView.Text = _currentUser.Surname ?? "";
            TxtFirstNameView.Text = _currentUser.FirstName ?? "";
            TxtMiddleNameView.Text = _currentUser.MiddleName ?? "";

            TxtUsername.Text = _currentUser.Username;
            TxtRoleDisplay.Text = GetRoleDisplayName(_currentUser.Role);

            // Телефон и Email
            TxtPhoneView.Text = string.IsNullOrWhiteSpace(_currentUser.Phone)
                ? "Не указан"
                : _currentUser.Phone;

            TxtEmailView.Text = string.IsNullOrWhiteSpace(_currentUser.Email)
                ? "Не указан"
                : _currentUser.Email;

            // Дата регистрации
            TxtCreatedAt.Text = _currentUser.CreatedAt.ToString("dd.MM.yyyy HH:mm");

            // ID (показываем только админу)
            if (_currentUser.Role.ToLower() == "admin")
            {
                PanelUserId.Visibility = Visibility.Visible;
                TxtUserId.Text = $"#{_currentUser.UserId}";
            }

            // Аватар
            string trimmedSurname = _currentUser.Surname?.Trim() ?? "";
            TxtAvatarInitial.Text = string.IsNullOrWhiteSpace(trimmedSurname) || trimmedSurname.Length == 0
                    ? "?"
                    : trimmedSurname.Substring(0, 1).ToUpper();
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
                    bgColor = Color.FromRgb(243, 232, 255);
                    borderColor = Color.FromRgb(216, 180, 254);
                    textColor = Color.FromRgb(126, 34, 206);
                    iconKind = PackIconKind.Shield;
                    description = "Полный доступ к системе: управление сотрудниками, бронирования, статистика";
                    break;

                case "manager":
                    bgColor = Color.FromRgb(219, 234, 254);
                    borderColor = Color.FromRgb(147, 197, 253);
                    textColor = Color.FromRgb(29, 78, 216);
                    iconKind = PackIconKind.AccountTie;
                    description = "Создание и редактирование бронирований, просмотр списка бронирований";
                    break;

                case "guest":
                    bgColor = Color.FromRgb(243, 244, 246);
                    borderColor = Color.FromRgb(229, 231, 235);
                    textColor = Color.FromRgb(55, 65, 81);
                    iconKind = PackIconKind.AccountOutline;
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
        // РЕДАКТИРОВАНИЕ ФИО (3 ПОЛЯ)
        // ═══════════════════════════════════════════════════════════

        private void BtnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            // Скрываем режим просмотра
            GridViewFullName.Visibility = Visibility.Collapsed;

            // Показываем режим редактирования
            PanelEditFullName.Visibility = Visibility.Visible;

            // Скрываем кнопку редактирования
            BtnEditProfile.Visibility = Visibility.Collapsed;

            // ✅ Заполняем отдельные поля
            TxtSurnameEdit.Text = _currentUser.Surname ?? "";
            TxtFirstNameEdit.Text = _currentUser.FirstName ?? "";
            TxtMiddleNameEdit.Text = _currentUser.MiddleName ?? "";

            TxtSurnameEdit.Focus();
        }


        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            string surname = TxtSurnameEdit.Text.Trim();
            string firstName = TxtFirstNameEdit.Text.Trim();
            string middleName = TxtMiddleNameEdit.Text.Trim();

            // ✅ Валидация фамилии через ValidationService
            var surnameValidation = Services.ValidationService.ValidateName(surname, "Фамилия");
            if (!surnameValidation.isValid)
            {
                MessageBox.Show(surnameValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSurnameEdit.Focus();
                return;
            }

            // ✅ Валидация имени через ValidationService
            var firstNameValidation = Services.ValidationService.ValidateName(firstName, "Имя");
            if (!firstNameValidation.isValid)
            {
                MessageBox.Show(firstNameValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtFirstNameEdit.Focus();
                return;
            }

            // ✅ Валидация отчества (опционально)
            if (!string.IsNullOrWhiteSpace(middleName))
            {
                var middleNameValidation = Services.ValidationService.ValidateName(middleName, "Отчество");
                if (!middleNameValidation.isValid)
                {
                    MessageBox.Show(middleNameValidation.errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtMiddleNameEdit.Focus();
                    return;
                }
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);
                    if (user != null)
                    {
                        // ✅ Сохраняем отдельные поля
                        user.Surname = surname;
                        user.FirstName = firstName;
                        user.MiddleName = middleName;

                        context.SaveChanges();

                        // Обновляем локальные данные
                        _currentUser.Surname = surname;
                        _currentUser.FirstName = firstName;
                        _currentUser.MiddleName = middleName;

                        App.CurrentUser.Surname = surname;
                        App.CurrentUser.FirstName = firstName;
                        App.CurrentUser.MiddleName = middleName;

                        // Обновляем отображение
                        TxtSurnameView.Text = surname;
                        TxtFirstNameView.Text = firstName;
                        TxtMiddleNameView.Text = middleName;
                        TxtUserFullName.Text = user.FullName;
                        TxtAvatarInitial.Text = !string.IsNullOrWhiteSpace(surname) && surname.Length > 0
                            ? surname.Substring(0, 1).ToUpper()
                            : "?";

                        // Возвращаем в режим просмотра
                        GridViewFullName.Visibility = Visibility.Visible;
                        PanelEditFullName.Visibility = Visibility.Collapsed;
                        BtnEditProfile.Visibility = Visibility.Visible;

                        // Обновляем главное окно
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow?.UpdateUserDisplay();

                        MessageBox.Show("ФИО успешно обновлено", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelEditProfile_Click(object sender, RoutedEventArgs e)
        {
            // Возвращаем в режим просмотра
            GridViewFullName.Visibility = Visibility.Visible;
            PanelEditFullName.Visibility = Visibility.Collapsed;
            BtnEditProfile.Visibility = Visibility.Visible;
        }

        // ═══════════════════════════════════════════════════════════
        // РЕДАКТИРОВАНИЕ ТЕЛЕФОНА
        // ═══════════════════════════════════════════════════════════

        private void BtnEditPhone_Click(object sender, RoutedEventArgs e)
        {
            TxtPhoneView.Visibility = Visibility.Collapsed;
            PanelEditPhone.Visibility = Visibility.Visible;
            BtnEditPhone.Visibility = Visibility.Collapsed;

            TxtPhoneEdit.Text = _currentUser.Phone;
            TxtPhoneEdit.Focus();
        }

        private void BtnSavePhone_Click(object sender, RoutedEventArgs e)
        {
            string newPhone = TxtPhoneEdit.Text.Trim();

            // ✅ Валидация телефона через ValidationService (если не пустой)
            if (!string.IsNullOrWhiteSpace(newPhone))
            {
                var phoneValidation = Services.ValidationService.ValidatePhone(newPhone);
                if (!phoneValidation.isValid)
                {
                    MessageBox.Show(phoneValidation.errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtPhoneEdit.Focus();
                    return;
                }
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);
                    if (user != null)
                    {
                        user.Phone = newPhone;
                        context.SaveChanges();

                        _currentUser.Phone = newPhone;
                        App.CurrentUser.Phone = newPhone;

                        TxtPhoneView.Text = string.IsNullOrWhiteSpace(newPhone) ? "Не указан" : newPhone;

                        TxtPhoneView.Visibility = Visibility.Visible;
                        PanelEditPhone.Visibility = Visibility.Collapsed;
                        BtnEditPhone.Visibility = Visibility.Visible;

                        MessageBox.Show("Телефон успешно обновлён", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelPhone_Click(object sender, RoutedEventArgs e)
        {
            TxtPhoneView.Visibility = Visibility.Visible;
            PanelEditPhone.Visibility = Visibility.Collapsed;
            BtnEditPhone.Visibility = Visibility.Visible;
        }

        // ═══════════════════════════════════════════════════════════
        // РЕДАКТИРОВАНИЕ EMAIL
        // ═══════════════════════════════════════════════════════════

        private void BtnEditEmail_Click(object sender, RoutedEventArgs e)
        {
            TxtEmailView.Visibility = Visibility.Collapsed;
            PanelEditEmail.Visibility = Visibility.Visible;
            BtnEditEmail.Visibility = Visibility.Collapsed;

            TxtEmailEdit.Text = _currentUser.Email;
            TxtEmailEdit.Focus();
        }

        private void BtnSaveEmail_Click(object sender, RoutedEventArgs e)
        {
            string newEmail = TxtEmailEdit.Text.Trim();

            // ✅ Валидация email через ValidationService (если не пустой)
            if (!string.IsNullOrWhiteSpace(newEmail))
            {
                var emailValidation = Services.ValidationService.ValidateEmail(newEmail);
                if (!emailValidation.isValid)
                {
                    MessageBox.Show(emailValidation.errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtEmailEdit.Focus();
                    return;
                }
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == _currentUser.UserId);
                    if (user != null)
                    {
                        user.Email = newEmail;
                        context.SaveChanges();

                        _currentUser.Email = newEmail;
                        App.CurrentUser.Email = newEmail;

                        TxtEmailView.Text = string.IsNullOrWhiteSpace(newEmail) ? "Не указан" : newEmail;

                        TxtEmailView.Visibility = Visibility.Visible;
                        PanelEditEmail.Visibility = Visibility.Collapsed;
                        BtnEditEmail.Visibility = Visibility.Visible;

                        MessageBox.Show("Email успешно обновлён", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelEmail_Click(object sender, RoutedEventArgs e)
        {
            TxtEmailView.Visibility = Visibility.Visible;
            PanelEditEmail.Visibility = Visibility.Collapsed;
            BtnEditEmail.Visibility = Visibility.Visible;
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
                MessageBox.Show("❌ Введите текущий пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCurrentPassword.Focus();
                return;
            }

            // ✅ Валидация нового пароля через ValidationService
            var passwordValidation = Services.ValidationService.ValidatePassword(newPassword);
            if (!passwordValidation.isValid)
            {
                MessageBox.Show(passwordValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNewPassword.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("❌ Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtConfirmPassword.Focus();
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

                    if (!Services.PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                    {
                        MessageBox.Show("Неверный текущий пароль", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    user.PasswordHash = Services.PasswordHasher.HashPassword(newPassword);
                    context.SaveChanges();

                    TxtCurrentPassword.Clear();
                    TxtNewPassword.Clear();
                    TxtConfirmPassword.Clear();

                    BtnChangePassword.Visibility = Visibility.Visible;
                    PanelPasswordChange.Visibility = Visibility.Collapsed;

                    MessageBox.Show("Пароль успешно изменён", "Успех",
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
        // СТАТИСТИКА
        // ═══════════════════════════════════════════════════════════

        private void InitializeStatistics()
        {
            GridStatistics.Children.Clear();
            GridStatistics.ColumnDefinitions.Clear();

            var stats = GetStatisticsByRole(_currentUser.Role);

            if (stats.Count == 0) return;

            for (int i = 0; i < stats.Count; i++)
            {
                GridStatistics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                if (i < stats.Count - 1)
                {
                    GridStatistics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
                }
            }

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
                            int totalStaff = context.Users
                                .Count(u => u.IsActive && (u.Role == "admin" || u.Role == "manager"));

                            int totalBookings = context.Bookings.Count();
                            int totalCabins = context.Cabins.Count(c => c.IsActive);

                            return new List<StatisticItem>
                            {
                                new StatisticItem
                                {
                                    Label = "Всего сотрудников",
                                    Value = totalStaff.ToString(),
                                    IconKind = PackIconKind.AccountGroup,
                                    Color = Color.FromRgb(147, 51, 234)
                                },
                                new StatisticItem
                                {
                                    Label = "Всего бронирований",
                                    Value = totalBookings.ToString(),
                                    IconKind = PackIconKind.CalendarMonth,
                                    Color = Color.FromRgb(37, 99, 235)
                                },
                                new StatisticItem
                                {
                                    Label = "Всего домиков",
                                    Value = totalCabins.ToString(),
                                    IconKind = PackIconKind.Home,
                                    Color = Color.FromRgb(22, 163, 74)
                                }
                            };

                        case "manager":
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
                                    Color = Color.FromRgb(37, 99, 235)
                                },
                                new StatisticItem
                                {
                                    Label = "В этом месяце",
                                    Value = thisMonthBookings.ToString(),
                                    IconKind = PackIconKind.ChartBar,
                                    Color = Color.FromRgb(22, 163, 74)
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
                                    Color = Color.FromRgb(107, 114, 128)
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

                return new List<StatisticItem>
                {
                    new StatisticItem
                    {
                        Label = "Статистика недоступна",
                        Value = "—",
                        IconKind = PackIconKind.AlertCircle,
                        Color = Color.FromRgb(239, 68, 68)
                    }
                };
            }
        }

        private class StatisticItem
        {
            public string Label { get; set; }
            public string Value { get; set; }
            public PackIconKind IconKind { get; set; }
            public Color Color { get; set; }
        }
    }
}