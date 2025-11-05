using System;
using System.Linq;

namespace AgroCulture.Services
{
    public class DatabaseService
    {
        /// <summary>
        /// Авторизация пользователя (ПРОСТАЯ ВЕРСИЯ для курсовой)
        /// </summary>
        public Users AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // ✅ ПРОСТАЯ ПРОВЕРКА: логин И пароль совпадают
                    var user = context.Users
                        .FirstOrDefault(u =>
                            u.Username == username &&
                            u.PasswordHash == password &&
                            u.IsActive == true);

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
                    return context.Users.FirstOrDefault(u => u.UserId == userId);
                }
            }
            catch
            {
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
            catch
            {
                return new System.Collections.Generic.List<Users>();
            }
        }
    }
}