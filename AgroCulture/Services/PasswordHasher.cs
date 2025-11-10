using System;
using System.Security.Cryptography;
using System.Text;

namespace AgroCulture.Services
{
    /// <summary>
    /// Сервис для безопасного хеширования паролей с использованием SHA256
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Хеширует пароль с использованием SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым");

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password.Trim());
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);

                // Конвертируем в строку HEX
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Проверка пароля путем сравнения хешей
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            string passwordHash = HashPassword(password);
            return passwordHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}