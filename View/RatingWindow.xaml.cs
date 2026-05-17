using HakatonApplication.Service;
using HakatonApplication.ViewModel;
using System.Windows;

namespace HakatonApplication.View
{
    public partial class RatingWindow : Window
    {
        public RatingViewModel ViewModel { get; }

        public RatingWindow(int taskId, int expertRegistrationId, IHakatonService service)
        {
            InitializeComponent();
            ViewModel = new RatingViewModel(taskId, expertRegistrationId, service);
            DataContext = ViewModel;
            ViewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }
}