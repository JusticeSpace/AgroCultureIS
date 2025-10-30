using System;
using System.Security.Cryptography;
using System.Text;

namespace AgroCulture.Services
{
    public static class PasswordHasher
    {
        /// <summary>
        /// Хеширование пароля SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым");

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Проверка пароля
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            string hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}