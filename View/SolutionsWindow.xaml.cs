using HakatonApplication.Service;
using HakatonApplication.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HakatonApplication.View
{
    public partial class SolutionsWindow : Window
    {
        public SolutionsViewModel ViewModel { get; }

        public SolutionsWindow(int taskId, int currentUserId, IHakatonService service)
        {
            InitializeComponent();
            ViewModel = new SolutionsViewModel(taskId, currentUserId, service);
            DataContext = ViewModel;
            ViewModel.CloseRequest += (s, e) => DialogResult = true;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string uri = e.Uri.OriginalString;
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                uri = "https://" + uri;
            }
            try
            {
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть ссылку: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            e.Handled = true;
        }
    }
}
