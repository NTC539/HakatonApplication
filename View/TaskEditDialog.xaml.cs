using HakatonApplication.Models;
using HakatonApplication.ViewModel;
using System.Windows;

namespace HakatonApplication.View
{
    public partial class TaskEditDialog : Window
    {
        public TaskEditViewModel ViewModel { get; }
        public StageTask? ResultTask => ViewModel.ResultTask;

        public TaskEditDialog(StageTask? existing = null, int? stageId = null)
        {
            InitializeComponent();
            ViewModel = new TaskEditViewModel(existing, stageId);
            DataContext = ViewModel;
            ViewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }
}