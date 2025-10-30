using System;
using System.Linq;

namespace AgroCulture.Services
{
    public class DatabaseService
    {
        /// <summary>
        /// Проверка подключения к БД
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // Попытка выполнить простой запрос
                    var count = context.Users.Count();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"❌ Ошибка подключения к БД:\n\n{ex.Message}\n\n" +
                    $"InnerException: {ex.InnerException?.Message}\n\n" +
                    "Проверьте:\n" +
                    "1. Запущен ли SQL Server\n" +
                    "2. Connection String в App.config\n" +
                    "3. Создана ли БД AgroCulture",
                    "Ошибка подключения",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Авторизация пользователя
        /// </summary>
        /// <returns>Объект Users или null</returns>
        public Users AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            // Хешируем введенный пароль
            string passwordHash = PasswordHasher.HashPassword(password);

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // LINQ запрос к БД
                    var user = context.Users
                        .Where(u => u.Username == username
                                 && u.PasswordHash == passwordHash
                                 && u.IsActive == true)
                        .FirstOrDefault();

                    return user;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка авторизации:\n{ex.Message}",
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        public Users GetUserById(int userId)
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    return context.Users
                        .Where(u => u.UserId == userId)
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка получения данных пользователя:\n{ex.Message}",
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Получить всех активных пользователей
        /// </summary>
        public System.Collections.Generic.List<Users> GetAllUsers()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    return context.Users
                        .Where(u => u.IsActive == true)
                        .OrderBy(u => u.FullName)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка получения списка пользователей:\n{ex.Message}",
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return new System.Collections.Generic.List<Users>();
            }
        }
    }
}