using System;
using System.Linq;

namespace AgroCulture.Services
{
    public class DatabaseService
    {
        /// <summary>
        /// Упрощенная авторизация для курсовой работы
        /// Сравнивает пароль напрямую с PasswordHash (без хеширования)
        /// </summary>
        public Users AuthenticateUser(string username, string password)
        {
            try
            {
                using (var context = new AgroCultureEntities())
                {
                    // ✅ ПРОСТОЕ СРАВНЕНИЕ: пароль == PasswordHash
                    var user = context.Users
                        .FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

                    return user; // Вернет null если не найден
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AUTH ERROR] {ex.Message}");
                return null;
            }
        }
    }
}