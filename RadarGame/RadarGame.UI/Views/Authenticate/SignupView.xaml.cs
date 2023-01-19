using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RadarGame.UI.ViewModels.Authenticate;

namespace RadarGame.UI.Views.Authenticate
{
    /// <summary>
    /// Interaction logic for SignUpView.xaml
    /// </summary>
    public partial class SignupView : UserControl
    {
        public SignupView()
        {
            SignupViewModel  viewModel= new SignupViewModel();
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}
