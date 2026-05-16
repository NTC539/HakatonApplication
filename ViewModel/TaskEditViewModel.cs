using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HakatonApplication.Models;
using System;

namespace HakatonApplication.ViewModel
{
    public partial class TaskEditViewModel : ObservableObject
    {
        private readonly StageTask? _existing;
        private readonly int? _stageId;

        [ObservableProperty]
        private string _description = "";

        [ObservableProperty]
        private bool _isSolutionsPublic;

        public StageTask? ResultTask { get; private set; }

        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public event EventHandler? CloseRequest;

        public TaskEditViewModel(StageTask? existing = null, int? stageId = null)
        {
            _existing = existing;
            _stageId = stageId;
            if (existing != null)
            {
                Description = existing.Description ?? "";
                IsSolutionsPublic = existing.IsSolutionsPublic == 1;
            }
            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(() => CloseRequest?.Invoke(this, EventArgs.Empty));
        }

        private void Ok()
        {
            ResultTask = _existing ?? new StageTask();
            ResultTask.Description = Description;
            ResultTask.IsSolutionsPublic = IsSolutionsPublic ? (short)(1) : (short)(0);
            if (_stageId.HasValue && _existing == null)
                ResultTask.StageId = _stageId.Value;
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}