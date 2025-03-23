using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLON.Models
{
    public class AuthUser
    {
        public static bool RegisterUser(string username, string password)
        {
            // здесь логика для регистрации
            return true;
        }

        // Заглушка для аутентификации пользователя
        public static bool AuthenticateUser(string username, string password)
        {
            // здесть тоже логика
            return true;
        }
    }
}