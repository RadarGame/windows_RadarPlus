using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RadarGame.UI.Views.Authenticate;

namespace RadarGame.UI.ViewModels.Authenticate
{
    public class AuthenticationViewModel
    {
        private static AuthenticationViewModel instatnce;
        private  AuthenticationView view;

        public AuthenticationViewModel()
        {
            Instatnce = this;
            view = new AuthenticationView();
            view.DataContext = this;


            view.ShowDialog();
        }

        public void ShotDown()
        {
            view.Close();
        }

        public static AuthenticationViewModel Instatnce
        {
            get => instatnce;
            set => instatnce = value;
        }
    }
}
