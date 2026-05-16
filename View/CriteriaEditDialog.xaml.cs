using HakatonApplication.Models;
using HakatonApplication.Service;
using HakatonApplication.ViewModel;
using System.Windows;

namespace HakatonApplication.View
{
    public partial class CriteriaEditDialog : Window
    {
        public CriteriaEditViewModel ViewModel { get; }
        public TaskCriterion? ResultCriteria => ViewModel.ResultCriteria;

        public CriteriaEditDialog(TaskCriterion? existing = null, int? taskId = null, IHakatonService? service = null)
        {
            InitializeComponent();
            ViewModel = new CriteriaEditViewModel(existing, taskId, service);
            DataContext = ViewModel;
            ViewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }
}