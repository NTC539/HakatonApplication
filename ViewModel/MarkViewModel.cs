using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HakatonApplication.DTO;
using HakatonApplication.Models;
using HakatonApplication.Service;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HakatonApplication.ViewModel
{
    public partial class MarksViewModel : ObservableObject
    {
        private readonly IHakatonService _service;
        private readonly int _taskId;
        private readonly int _expertRegistrationId; 
        public bool IsEditable => _expertRegistrationId != 0;

        [ObservableProperty] private ObservableCollection<TeamForRatingDto> _teams = new();
        [ObservableProperty] private TeamForRatingDto? _selectedTeam;
        [ObservableProperty] private ObservableCollection<CriteriaMarkDto> _criteria = new();

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CloseCommand { get; }
        public event EventHandler? CloseRequest;

        public MarksViewModel(int taskId, IHakatonService service, int expertRegistrationId = 0)
        {
            _taskId = taskId;
            _service = service;
            _expertRegistrationId = expertRegistrationId;
            SaveCommand = new RelayCommand(SaveAsync);
            CloseCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
            _ = LoadTeamsAsync();
        }

        private async Task LoadTeamsAsync()
        {
            var teams = await _service.GetTeamsWithSolutionForTaskAsync(_taskId);
            Teams = new ObservableCollection<TeamForRatingDto>(teams);
            if (Teams.Any())
                SelectedTeam = Teams.First();
        }

        partial void OnSelectedTeamChanged(TeamForRatingDto? oldValue, TeamForRatingDto? newValue)
        {
            if (newValue != null)
                _ = LoadCriteriaAsync(newValue.TeamId);
        }

        private async Task LoadCriteriaAsync(int teamId)
        {
            if (IsEditable)
            {
                var marks = await _service.GetMarksForTeamAndTaskAsync(teamId, _taskId, _expertRegistrationId);
                Criteria = new ObservableCollection<CriteriaMarkDto>(marks);
            }
            else
            {
                var marks = await _service.GetDetailedMarksForTeamTaskAsync(teamId, _taskId);
                Criteria = new ObservableCollection<CriteriaMarkDto>(marks);
            }
        }

        private async void SaveAsync()
        {
            if (SelectedTeam == null)
            {
                MessageBox.Show("Выберите команду.");
                return;
            }
            await _service.SaveMarksAsync(_taskId, SelectedTeam.TeamId, _expertRegistrationId, Criteria.ToList());
            MessageBox.Show("Оценки сохранены.");
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}