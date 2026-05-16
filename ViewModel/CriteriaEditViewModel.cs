using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HakatonApplication.Models;
using HakatonApplication.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace HakatonApplication.ViewModel
{
    public partial class CriteriaEditViewModel : ObservableObject
    {
        private readonly TaskCriterion? _existing;
        private readonly int? _taskId;
        private readonly IHakatonService? _service;

        [ObservableProperty]
        private string _description = "";

        [ObservableProperty]
        private decimal? _maxMark;

        [ObservableProperty]
        private ObservableCollection<Criterion> _availableCriteria = new();

        [ObservableProperty]
        private Criterion? _selectedCriteria;

        [ObservableProperty]
        private string _newCriteriaName = "";

        public TaskCriterion? ResultCriteria { get; private set; }

        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public event EventHandler? CloseRequest;

        public CriteriaEditViewModel(TaskCriterion? existing = null, int? taskId = null, IHakatonService? service = null)
        {
            _existing = existing;
            _taskId = taskId;
            _service = service;

            if (existing != null)
            {
                Description = existing.Description ?? "";
                MaxMark = existing.MaxMark;
                if (existing.Criteria != null)
                {
                    SelectedCriteria = existing.Criteria;
                    NewCriteriaName = existing.Criteria.Name ?? "";
                }
            }

            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));

            _ = LoadCriteriaAsync();
        }

        private async Task LoadCriteriaAsync()
        {
            if (_service != null)
            {
                var list = await _service.GetAllCriteriaAsync();
                AvailableCriteria = new ObservableCollection<Criterion>(list);
            }
        }

        private void Ok()
        {
            // Определяем критерий
            Criterion targetCriteria;
            if (SelectedCriteria != null && (string.IsNullOrEmpty(NewCriteriaName) || SelectedCriteria.Name == NewCriteriaName))
            {
                targetCriteria = SelectedCriteria;
            }
            else if (!string.IsNullOrEmpty(NewCriteriaName))
            {
                targetCriteria = new Criterion { Name = NewCriteriaName };
            }
            else
            {
                // Нет ни выбранного, ни введённого имени – можно показать MessageBox
                System.Windows.MessageBox.Show("Выберите или введите название критерия.", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            ResultCriteria = _existing ?? new TaskCriterion();
            ResultCriteria.Description = Description;
            ResultCriteria.MaxMark = MaxMark;
            ResultCriteria.Criteria = targetCriteria;

            if (_taskId.HasValue && _existing == null)
                ResultCriteria.TaskId = _taskId.Value;

            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}