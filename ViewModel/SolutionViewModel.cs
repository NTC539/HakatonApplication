using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HakatonApplication.DTO;
using HakatonApplication.Service;
using HakatonApplication.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HakatonApplication.ViewModel
{

    public partial class SolutionsViewModel : ObservableObject
    {
        private readonly IHakatonService _service;
        private readonly int _taskId;
        private readonly int _currentUserId;
        private bool _isOrganizerOrExpert;
        private int? _userTeamId;

        [ObservableProperty] private ObservableCollection<SolutionDto> _solutions = new();
        [ObservableProperty] private bool _canAddSolution;
        [ObservableProperty] private bool _canEdit;
        [ObservableProperty] private string _infoMessage = "";

        public IAsyncRelayCommand AddSolutionCommand { get; }
        public IAsyncRelayCommand<SolutionDto> EditSolutionCommand { get; }
        public event EventHandler? CloseRequest;

        public SolutionsViewModel(int taskId, int currentUserId, IHakatonService service)
        {
            _taskId = taskId;
            _currentUserId = currentUserId;
            _service = service;
            AddSolutionCommand = new AsyncRelayCommand(AddSolutionAsync);
            EditSolutionCommand = new AsyncRelayCommand<SolutionDto>(EditSolutionAsync);
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var role = await _service.GetUserRoleOnHakatonByTaskAsync(_taskId, _currentUserId);
            _isOrganizerOrExpert = role == 2 || role == 3;
            _userTeamId = await _service.GetUserTeamIdOnHakatonByTaskAsync(_taskId, _currentUserId);
            var solutions = await _service.GetSolutionsForTaskAsync(_taskId, _currentUserId, _isOrganizerOrExpert);
            Solutions = new ObservableCollection<SolutionDto>(solutions);

            if (!_isOrganizerOrExpert && _userTeamId.HasValue)
            {
                var hasSolution = await _service.HasUserSolutionForTaskAsync(_taskId, _userTeamId.Value);
                CanAddSolution = !hasSolution;
                CanEdit = true;  // может редактировать своё решение (будет проверено в команде редактирования)
            }
            else if (_isOrganizerOrExpert)
            {
                CanEdit = true; // эксперты/организаторы не редактируют чужие решения, но кнопка будет скрыта логикой в команде
            }
        }

        private async Task AddSolutionAsync()
        {
            if (!_userTeamId.HasValue)
            {
                MessageBox.Show("Вы не состоите в команде.");
                return;
            }
            var dialog = new SolutionEditDialog(_taskId, _userTeamId.Value);
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                await _service.AddOrUpdateSolutionAsync(dialog.Result);
                await LoadDataAsync();
                InfoMessage = "Решение сохранено.";
            }
        }

        private async Task EditSolutionAsync(SolutionDto solution)
        {
            var dto = await _service.GetSolutionForEditAsync(solution.Id, _currentUserId);
            if (dto == null)
            {
                MessageBox.Show("У вас нет прав редактировать это решение.");
                return;
            }
            var dialog = new SolutionEditDialog(dto);
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                await _service.AddOrUpdateSolutionAsync(dialog.Result);
                await LoadDataAsync();
                InfoMessage = "Решение обновлено.";
            }
        }
        
    }
}
