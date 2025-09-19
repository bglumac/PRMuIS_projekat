using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client.Classes
{
    public class AuthHandler
    {
        private static bool logged = false;

        public static void setLogged(bool logged)
        {
            AuthHandler.logged = logged;
        }

        public static bool getLogged()
        {
            return logged; 
        }
    }
}
