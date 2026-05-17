using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HakatonApplication.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HakatonApplication.ViewModel
{
    public partial class SolutionEditViewModel : ObservableObject
    {
        private readonly int? _taskId;
        private readonly int? _teamId;
        private readonly SolutionEditDto? _existing;

        [ObservableProperty] private string _name = "";
        [ObservableProperty] private string _description = "";
        [ObservableProperty] private string _source = "";

        public SolutionEditDto? Result { get; private set; }
        public IRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public event EventHandler? CloseRequest;

        public SolutionEditViewModel(int taskId, int teamId)
        {
            _taskId = taskId;
            _teamId = teamId;
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
        }

        public SolutionEditViewModel(SolutionEditDto existing)
        {
            _existing = existing;
            Name = existing.Name;
            Description = existing.Description;
            Source = existing.Source;
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
        }

        private void Save()
        {
            if (_existing != null)
            {
                _existing.Name = Name;
                _existing.Description = Description;
                _existing.Source = Source;
                Result = _existing;
            }
            else
            {
                if (!_taskId.HasValue || !_teamId.HasValue) return;
                Result = new SolutionEditDto
                {
                    TaskId = _taskId.Value,
                    TeamId = _teamId.Value,
                    Name = Name,
                    Description = Description,
                    Source = Source
                };
            }
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
