using System.Threading.Tasks;
using System.Windows;
using HakatonApplication.ViewModel;

namespace HakatonApplication.View
{
    public partial class LoginWindow : Window
    {
        public LoginViewModel ViewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            ViewModel = viewModel;
        }

        private void SwitchMode_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SwitchMode();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            string password = LoginPasswordBox.Password;
            await ViewModel.LoginWithPasswordAsync(password);
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            string password = RegPasswordBox.Password;
            string confirm = RegConfirmPasswordBox.Password;
            await ViewModel.RegisterWithPasswordsAsync(password, confirm);
        }
    }
}