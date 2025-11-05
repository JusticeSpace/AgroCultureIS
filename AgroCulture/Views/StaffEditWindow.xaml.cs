using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AgroCulture.Views
{
    /// <summary>
    /// Логика взаимодействия для StaffEditWindow.xaml
    /// </summary>
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

                    TxtUsername.Text = user.Username;
                    TxtFullName.Text = user.FullName;

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

            if (string.IsNullOrWhiteSpace(TxtFullName.Text))
            {
                MessageBox.Show("Введите ФИО", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtFullName.Focus();
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
                    string fullName = TxtFullName.Text.Trim();

                    // Проверка уникальности логина
                    if (context.Users.Any(u => u.Username == username && u.UserId != EditUserId))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        TxtUsername.Focus();
                        return;
                    }

                    user.Role = role;
                    user.Username = username;
                    user.FullName = fullName;

                    context.SaveChanges();

                    MessageBox.Show($"Данные сотрудника {fullName} успешно обновлены", "Успех",
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

