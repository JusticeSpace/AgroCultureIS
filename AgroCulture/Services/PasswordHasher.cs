using System;

namespace AgroCulture.Services
{
    /// <summary>
    /// Упрощенная версия для курсовой работы
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Возвращает пароль как есть (без хеширования)
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым");

            return password.Trim(); // ✅ ПРОСТАЯ ВЕРСИЯ
        }

        /// <summary>
        /// Проверка пароля (простое сравнение)
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            return password?.Trim() == hash?.Trim();
        }
    }
}