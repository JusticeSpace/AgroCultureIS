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

                    // ✅ Загружаем отдельные поля
                    TxtUsername.Text = user.Username;
                    TxtSurname.Text = user.Surname ?? "";
                    TxtFirstName.Text = user.FirstName ?? "";
                    TxtMiddleName.Text = user.MiddleName ?? "";

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

            // ✅ Проверяем отдельные поля
            if (string.IsNullOrWhiteSpace(TxtSurname.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSurname.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtFirstName.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtFirstName.Focus();
                return false;
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

                    // ✅ Сохраняем отдельные поля
                    user.Role = role;
                    user.Username = username;
                    user.Surname = TxtSurname.Text.Trim();
                    user.FirstName = TxtFirstName.Text.Trim();
                    user.MiddleName = TxtMiddleName.Text.Trim();

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