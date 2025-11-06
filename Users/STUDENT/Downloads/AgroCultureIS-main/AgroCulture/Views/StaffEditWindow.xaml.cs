using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AgroCulture.Views
{
    public partial class StaffEditWindow : Window
    {
        public int EditUserId { get; set; }
        public bool DialogResultSuccess { get; private set; } = false;

        public StaffEditWindow()
        {
            InitializeComponent();
        }

        public StaffEditWindow(int userId) : this()
        {
            EditUserId = userId;
            this.Loaded += StaffEditWindow_Loaded;
        }

        private void StaffEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= StaffEditWindow_Loaded;
            LoadUserData();
        }

        private void LoadUserData()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    var user = context.Users.FirstOrDefault(u => u.UserId == EditUserId);

                    if (user == null)
                    {
                        MessageBox.Show("Сотрудник не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);

                        DialogResultSuccess = false;
                        this.Close();
                        return;
                    }

                    TxtUsername.Text = user.Username;
                    TxtFullName.Text = user.FullName;
                    TxtPhone.Text = user.Phone ?? "";
                    TxtEmail.Text = user.Email ?? "";

                    CmbRole.SelectedIndex = user.Role == "admin" ? 1 : 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                DialogResultSuccess = false;
                this.Close();
            }
        }

        // ✅ НОВЫЙ ОБРАБОТЧИК: Показать/скрыть поле пароля
        private void ChkChangePassword_CheckedChanged(object sender, RoutedEventArgs e)
        {
            PasswordPanel.Visibility = ChkChangePassword.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Очищаем пароль при снятии галочки
            if (ChkChangePassword.IsChecked == false)
            {
                TxtNewPassword.Clear();
            }
        }

        private bool ValidateForm()
        {
            if (CmbRole.SelectedItem == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbRole.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtUsername.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtUsername.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtFullName.Text))
            {
                MessageBox.Show("Введите ФИО", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtFullName.Focus();
                return false;
            }

            // ✅ НОВАЯ ВАЛИДАЦИЯ: Проверка пароля, если чекбокс включен
            if (ChkChangePassword.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(TxtNewPassword.Password))
                {
                    MessageBox.Show("Введите новый пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtNewPassword.Focus();
                    return false;
                }

                if (TxtNewPassword.Password.Length < 4)
                {
                    MessageBox.Show("Пароль должен содержать минимум 4 символа", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtNewPassword.Focus();
                    return false;
                }
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
                    var user = context.Users.FirstOrDefault(u => u.UserId == EditUserId);

                    if (user == null)
                    {
                        MessageBox.Show("Сотрудник не найден", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string role = ((ComboBoxItem)CmbRole.SelectedItem).Tag.ToString();
                    string username = TxtUsername.Text.Trim();
                    string fullName = TxtFullName.Text.Trim();
                    string phone = TxtPhone.Text.Trim();
                    string email = TxtEmail.Text.Trim();

                    // Проверка уникальности логина
                    if (context.Users.Any(u => u.Username == username && u.UserId != EditUserId))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        TxtUsername.Focus();
                        return;
                    }

                    // Обновление основных данных
                    user.Role = role;
                    user.Username = username;
                    user.FullName = fullName;
                    user.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;
                    user.Email = string.IsNullOrWhiteSpace(email) ? null : email;

                    // ✅ НОВОЕ: Обновление пароля (если чекбокс включен)
                    bool passwordChanged = false;
                    if (ChkChangePassword.IsChecked == true)
                    {
                        user.PasswordHash = TxtNewPassword.Password.Trim();
                        passwordChanged = true;
                    }

                    context.SaveChanges();

                    // ✅ НОВОЕ: Разные сообщения в зависимости от того, менялся ли пароль
                    string successMessage = passwordChanged
                        ? $"Данные сотрудника {fullName} успешно обновлены.\n\nНовый пароль: {TxtNewPassword.Password}"
                        : $"Данные сотрудника {fullName} успешно обновлены";

                    MessageBox.Show(successMessage, "Успех",
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