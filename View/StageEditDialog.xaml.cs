using HakatonApplication.Models;
using HakatonApplication.Service;
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
    /// Логика взаимодействия для StageEditDialog.xaml
    /// </summary>
    public partial class StageEditDialog : Window
    {
        public StageEditViewModel ViewModel { get; }
        public Stage? ResultStage => ViewModel.ResultStage;

        public StageEditDialog(IHakatonService service, Stage ? existingStage = null)
        {
            InitializeComponent();
            ViewModel = new StageEditViewModel(service, existingStage);
            DataContext = ViewModel;
            ViewModel.CloseRequest += (s, e) => DialogResult = true;
        }
    }
}