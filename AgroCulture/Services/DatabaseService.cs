using System;
using System.Linq;
using System.Diagnostics;

namespace AgroCulture.Services
{
    public class DatabaseService
    {
        /// <summary>
        /// Авторизация с подробным логированием
        /// </summary>
        public Users AuthenticateUser(string username, string password)
        {
            Debug.WriteLine("========================================");
            Debug.WriteLine($"[AUTH] Попытка входа: '{username}'");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Debug.WriteLine("[AUTH] ❌ Пустой логин или пароль");
                return null;
            }

            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // 1️⃣ Ищем пользователя по логину
                    var user = context.Users
                        .FirstOrDefault(u => u.Username == username);

                    if (user == null)
                    {
                        Debug.WriteLine($"[AUTH] ❌ Пользователь '{username}' НЕ НАЙДЕН в БД");

                        // Показываем всех пользователей для отладки
                        var allUsers = context.Users.Select(u => u.Username).ToList();
                        Debug.WriteLine($"[AUTH] Доступные пользователи: {string.Join(", ", allUsers)}");

                        return null;
                    }

                    Debug.WriteLine($"[AUTH] ✅ Пользователь найден: {user.Username}");
                    Debug.WriteLine($"[AUTH] - Роль: {user.Role}");
                    Debug.WriteLine($"[AUTH] - IsActive: {user.IsActive}");
                    Debug.WriteLine($"[AUTH] - PasswordHash в БД: '{user.PasswordHash}'");
                    Debug.WriteLine($"[AUTH] - Введенный пароль: '{password}'");

                    // 2️⃣ Проверяем активность
                    if (!user.IsActive)
                    {
                        Debug.WriteLine("[AUTH] ❌ Пользователь НЕАКТИВЕН");
                        return null;
                    }

                    // 3️⃣ Проверяем пароль (убираем пробелы с обеих сторон)
                    string dbPassword = user.PasswordHash?.Trim() ?? "";
                    string inputPassword = password.Trim();

                    Debug.WriteLine($"[AUTH] Сравнение:");
                    Debug.WriteLine($"[AUTH] - БД (после trim): '{dbPassword}' (длина: {dbPassword.Length})");
                    Debug.WriteLine($"[AUTH] - Ввод (после trim): '{inputPassword}' (длина: {inputPassword.Length})");

                    if (dbPassword == inputPassword)
                    {
                        Debug.WriteLine("[AUTH] ✅✅✅ УСПЕШНАЯ АВТОРИЗАЦИЯ!");
                        Debug.WriteLine($"[AUTH] Вход выполнен: {user.Username} ({user.Role})");
                        Debug.WriteLine("========================================");
                        return user;
                    }
                    else
                    {
                        Debug.WriteLine("[AUTH] ❌ ПАРОЛЬ НЕ СОВПАДАЕТ!");

                        // Побайтовое сравнение для отладки
                        Debug.WriteLine("[AUTH] Побайтовое сравнение:");
                        for (int i = 0; i < Math.Max(dbPassword.Length, inputPassword.Length); i++)
                        {
                            char dbChar = i < dbPassword.Length ? dbPassword[i] : ' ';
                            char inputChar = i < inputPassword.Length ? inputPassword[i] : ' ';
                            Debug.WriteLine($"[AUTH]   [{i}] БД: '{dbChar}' ({(int)dbChar}) vs Ввод: '{inputChar}' ({(int)inputChar})");
                        }

                        Debug.WriteLine("========================================");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUTH] ❌❌❌ ОШИБКА: {ex.Message}");
                Debug.WriteLine($"[AUTH] StackTrace: {ex.StackTrace}");
                Debug.WriteLine("========================================");

                System.Windows.MessageBox.Show(
                    $"Ошибка авторизации:\n\n{ex.Message}\n\nПодробности в Output окне Visual Studio.",
                    "Ошибка",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        public Users GetUserById(int userId)
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    return context.Users.FirstOrDefault(u => u.UserId == userId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] Ошибка GetUserById: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ✅ Получить всех активных пользователей
        /// </summary>
        public System.Collections.Generic.List<Users> GetAllUsers()
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    return context.Users
                        .Where(u => u.IsActive == true)
                        .OrderBy(u => u.Surname)
                        .ThenBy(u => u.FirstName)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DB] Ошибка GetAllUsers: {ex.Message}");
                return new System.Collections.Generic.List<Users>();
            }
        }
    }
}