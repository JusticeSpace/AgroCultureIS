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
            Loaded += StaffEditWindow_Loaded;
        }

        private void StaffEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
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
                        this.Close();
                        return;
                    }

                    // ✅ Загружаем все поля
                    TxtUsername.Text = user.Username;
                    TxtSurname.Text = user.Surname ?? "";
                    TxtFirstName.Text = user.FirstName ?? "";
                    TxtMiddleName.Text = user.MiddleName ?? "";
                    TxtPhone.Text = user.Phone ?? "";              // ✅ НОВОЕ
                    TxtEmail.Text = user.Email ?? "";              // ✅ НОВОЕ

                    // Роль
                    if (user.Role == "admin")
                    {
                        CmbRole.SelectedIndex = 1;
                    }
                    else
                    {
                        CmbRole.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private bool ValidateForm()
        {
            if (CmbRole.SelectedItem == null)
            {
                MessageBox.Show("❌ Выберите роль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CmbRole.Focus();
                return false;
            }

            // ✅ Валидация логина через ValidationService
            var usernameValidation = Services.ValidationService.ValidateUsername(TxtUsername.Text);
            if (!usernameValidation.isValid)
            {
                MessageBox.Show(usernameValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtUsername.Focus();
                return false;
            }

            // ✅ Валидация фамилии через ValidationService
            var surnameValidation = Services.ValidationService.ValidateName(TxtSurname.Text, "Фамилия");
            if (!surnameValidation.isValid)
            {
                MessageBox.Show(surnameValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSurname.Focus();
                return false;
            }

            // ✅ Валидация имени через ValidationService
            var firstNameValidation = Services.ValidationService.ValidateName(TxtFirstName.Text, "Имя");
            if (!firstNameValidation.isValid)
            {
                MessageBox.Show(firstNameValidation.errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtFirstName.Focus();
                return false;
            }

            // ✅ Валидация отчества (опционально)
            if (!string.IsNullOrWhiteSpace(TxtMiddleName.Text))
            {
                var middleNameValidation = Services.ValidationService.ValidateName(TxtMiddleName.Text, "Отчество");
                if (!middleNameValidation.isValid)
                {
                    MessageBox.Show(middleNameValidation.errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtMiddleName.Focus();
                    return false;
                }
            }

            // ✅ Валидация Email через ValidationService (если заполнен)
            if (!string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                var emailValidation = Services.ValidationService.ValidateEmail(TxtEmail.Text);
                if (!emailValidation.isValid)
                {
                    MessageBox.Show(emailValidation.errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtEmail.Focus();
                    return false;
                }
            }

            // ✅ Валидация телефона через ValidationService (если заполнен)
            if (!string.IsNullOrWhiteSpace(TxtPhone.Text))
            {
                var phoneValidation = Services.ValidationService.ValidatePhone(TxtPhone.Text);
                if (!phoneValidation.isValid)
                {
                    MessageBox.Show(phoneValidation.errorMessage, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtPhone.Focus();
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

                    // Проверка уникальности логина
                    if (context.Users.Any(u => u.Username == username && u.UserId != EditUserId))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        TxtUsername.Focus();
                        return;
                    }

                    // ✅ Сохраняем все поля
                    user.Role = role;
                    user.Username = username;
                    user.Surname = TxtSurname.Text.Trim();
                    user.FirstName = TxtFirstName.Text.Trim();
                    user.MiddleName = TxtMiddleName.Text.Trim();
                    user.Phone = TxtPhone.Text.Trim();             // ✅ НОВОЕ
                    user.Email = TxtEmail.Text.Trim();             // ✅ НОВОЕ

                    context.SaveChanges();

                    MessageBox.Show($"Данные сотрудника {user.FullName} успешно обновлены", "Успех",
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