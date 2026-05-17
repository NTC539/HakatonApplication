using HakatonApplication.DTO;
using HakatonApplication.ViewModel;
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
using System.Windows.Shapes;

namespace HakatonApplication.View
{
    /// <summary>
    /// Логика взаимодействия для SolutionEditDialog.xaml
    /// </summary>
    public partial class SolutionEditDialog : Window
    {
        public SolutionEditViewModel ViewModel { get; }
        public SolutionEditDto? Result => ViewModel.Result;

        public SolutionEditDialog(int taskId, int teamId)
        {
            InitializeComponent();
            ViewModel = new SolutionEditViewModel(taskId, teamId);
            DataContext = ViewModel;
            ViewModel.CloseRequest += (s, e) => DialogResult = true;
        }

        public SolutionEditDialog(SolutionEditDto existing)
        {
            InitializeComponent();
            ViewModel = new SolutionEditViewModel(existing);
            DataContext = ViewModel;
            ViewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }

}
