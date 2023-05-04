using System.Text;

namespace PlanetIT.Domain.Helpers
{
    public static class TokenAuthorizationHelper
    {
        // Кодирует токен авторизации
        public static string Encode(string email, string password)
        {
            string token = $"{email}:{password}";
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            return Convert.ToBase64String(tokenBytes);
        }
        // Декодирует токен авторизации
        public static (string username, string password) Decode(string token)
        {
            byte[] tokenBytes = Convert.FromBase64String(token);
            string tokenString = Encoding.UTF8.GetString(tokenBytes);
            string[] parts = tokenString.Split(':');
            return (parts[0], parts[1]);
        }
    }
}