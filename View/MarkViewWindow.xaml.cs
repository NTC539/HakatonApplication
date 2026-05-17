using HakatonApplication.Service;
using HakatonApplication.ViewModel;
using System.Windows;

namespace HakatonApplication.View
{
    public partial class MarksViewWindow : Window
    {
        public MarksViewModel ViewModel { get; }

        public MarksViewWindow(int taskId, IHakatonService service, int expertRegistrationId = 0)
        {
            InitializeComponent();
            ViewModel = new MarksViewModel(taskId, service, expertRegistrationId);
            DataContext = ViewModel;
            ViewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }
}