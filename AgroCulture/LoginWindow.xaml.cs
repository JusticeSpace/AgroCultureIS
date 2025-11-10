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
        // АВТОРИЗАЦИЯ
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
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            // ✅ Авторизация через Entity Framework
            Users user = App.Database.AuthenticateUser(username, password);

            if (user != null)
            {
                // ✅ ЛОГИРОВАНИЕ для отладки
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Успешный вход: {user.Username} ({user.Role})");

                // Успешная авторизация
                App.CurrentUser = user;

                // Открываем главное окно
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Закрываем окно входа
                this.Close();
            }
            else
            {
                // ✅ ЛОГИРОВАНИЕ для отладки
                System.Diagnostics.Debug.WriteLine($"[LOGIN] ОШИБКА: Неверные данные для {username}");

                // Неверные данные
                MessageBox.Show(
                    $"Неверный логин или пароль.\n\nВы ввели:\nЛогин: {username}\nПароль: {password}\n\nПроверьте правильность ввода.",
                    "Ошибка авторизации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                PasswordBox.Clear();
                PasswordBox.Focus();
            }
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

        // ✅ ВХОД ГОСТЕМ - обычная проверка через БД
        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[LOGIN] Попытка входа как гость");

            // Проверяем через БД
            Users user = App.Database.AuthenticateUser("guest", "guest");

            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] ✅ Гость авторизован: {user.Username} ({user.Role})");

                App.CurrentUser = user;

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                this.Close();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[LOGIN] ❌ Ошибка входа гостя");

                MessageBox.Show(
                    "Не удалось войти в гостевой режим.\n\nПроверьте наличие пользователя 'guest' в базе данных.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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