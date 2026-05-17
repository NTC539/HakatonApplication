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
    public partial class RatingViewModel : ObservableObject
    {
        private readonly IHakatonService _service;
        private readonly int _taskId;
        private readonly int _expertRegistrationId;

        [ObservableProperty] private ObservableCollection<TeamForRatingDto> _teams = new();
        [ObservableProperty] private TeamForRatingDto? _selectedTeam;
        [ObservableProperty] private ObservableCollection<CriteriaMarkDto> _criteria = new();

        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public event EventHandler? CloseRequest;

        public RatingViewModel(int taskId, int expertRegistrationId, IHakatonService service)
        {
            _taskId = taskId;
            _expertRegistrationId = expertRegistrationId;
            _service = service;
            SaveCommand = new RelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
            _ = LoadTeamsAsync();
        }

        private async Task LoadTeamsAsync()
        {
            var teams = await _service.GetTeamsWithSolutionForTaskAsync(_taskId);
            Teams = new ObservableCollection<TeamForRatingDto>(teams);
        }

        partial void OnSelectedTeamChanged(TeamForRatingDto? oldValue, TeamForRatingDto? newValue)
        {
            if (newValue != null)
                _ = LoadCriteriaAsync(newValue.TeamId);
            else
                Criteria.Clear();
        }

        private async Task LoadCriteriaAsync(int teamId)
        {
            var criteria = await _service.GetMarksForTeamAndTaskAsync(teamId, _taskId, _expertRegistrationId);
            Criteria = new ObservableCollection<CriteriaMarkDto>(criteria);
        }

        private async void SaveAsync()
        {
            if (SelectedTeam == null)
            {
                MessageBox.Show("Выберите команду.");
                return;
            }
            await _service.SaveMarksAsync(_taskId, SelectedTeam.TeamId, _expertRegistrationId, Criteria.ToList());
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}