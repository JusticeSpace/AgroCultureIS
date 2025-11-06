using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AgroCulture
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.Default;

            // Обработчик Enter в полях
            LoginTextBox.KeyDown += Input_KeyDown;
            PasswordBox.KeyDown += Input_KeyDown;

            // Очистка placeholder при фокусе
            LoginTextBox.GotFocus += LoginTextBox_GotFocus;
            LoginTextBox.LostFocus += LoginTextBox_LostFocus;
        }

        // ========================================
        // АВТОРИЗАЦИЯ (ADMIN / MANAGER)
        // ========================================

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformLogin();
            }
        }

        private void PerformLogin()
        {
            string username = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            // Валидация
            if (string.IsNullOrWhiteSpace(username) || username == "Введите логин")
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            Users user = App.Database.AuthenticateUser(username, password);

            if (user != null)
            {
                App.CurrentUser = user;

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "Неверный логин или пароль.\n\nПроверьте правильность ввода.",
                    "Ошибка авторизации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                PasswordBox.Clear();
                PasswordBox.Focus();
            }
        }

        // ========================================
        // ✅ ГОСТЕВОЙ ВХОД (БЕЗ ПАРОЛЯ)
        // ========================================

        private void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            PerformGuestLogin();
        }

        /// <summary>
        /// Вход в гостевом режиме (без авторизации)
        /// </summary>
        private void PerformGuestLogin()
        {
            // ✅ СОЗДАЕМ ВРЕМЕННОГО ГОСТЯ В ПАМЯТИ
            App.CurrentUser = new Users
            {
                UserId = 0,
                Username = "guest",
                FullName = "Гостевой режим",
                Role = "guest",
                CreatedAt = DateTime.Now,
                IsActive = true,
                PasswordHash = "",
                Phone = "",
                Email = ""
            };

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        // ========================================
        // КЛИК ПО ТЕСТОВЫМ АККАУНТАМ
        // ========================================

        private void AdminCard_Click(object sender, MouseButtonEventArgs e)
        {
            FillCredentials("admin", "admin");
        }

        private void ManagerCard_Click(object sender, MouseButtonEventArgs e)
        {
            FillCredentials("manager", "manager");
        }

        private void GuestCard_Click(object sender, MouseButtonEventArgs e)
        {
            // ✅ МОЖНО ОСТАВИТЬ ИЛИ УДАЛИТЬ ЭТОТ МЕТОД
            FillCredentials("guest", "guest");
        }

        private void FillCredentials(string username, string password)
        {
            LoginTextBox.Text = username;
            LoginTextBox.Foreground = System.Windows.Media.Brushes.Black;
            PasswordBox.Password = password;
            LoginButton.Focus();
        }

        // ========================================
        // PLACEHOLDER ДЛЯ ЛОГИНА
        // ========================================

        private void LoginTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LoginTextBox.Text == "Введите логин")
            {
                LoginTextBox.Text = "";
                LoginTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void LoginTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                LoginTextBox.Text = "Введите логин";
                LoginTextBox.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#717182"));
            }
        }
    }
}