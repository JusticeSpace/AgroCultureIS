using System;

namespace AgroCulture.Services
{
    public static class PasswordHasher
    {
        /// <summary>
        /// Возвращает пароль как есть (без хеширования)
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым");

            return password.Trim(); // ✅ Для курсовой - возвращаем как есть
        }

        /// <summary>
        /// Проверка пароля (простое сравнение)
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            return password.Trim() == hash.Trim();
        }
    }
}