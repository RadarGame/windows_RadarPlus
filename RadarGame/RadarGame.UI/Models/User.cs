using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarGame.UI.Models
{
    public class User
    {
        private static User _instance;
        private string username = "";
        private string password = "";
        private string uniqueId = "";
        private User() { }

        public static User GetUser()
        {
            if (_instance == null)
                _instance = new User();

            return _instance;
        }

        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string UniqueId { get => uniqueId; set => uniqueId = value; }
    }
}
