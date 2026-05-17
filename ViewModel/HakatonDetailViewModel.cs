using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HakatonApplication.DTO;
using HakatonApplication.Message;
using HakatonApplication.Models;
using HakatonApplication.Service;
using HakatonApplication.View;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace HakatonApplication.ViewModel
{
    public partial class HakatonDetailViewModel : ViewModelBase
    {
        private readonly IHakatonService _hakatonService;

        [ObservableProperty]
        private int _hakatonId;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _editName = string.Empty;

        [ObservableProperty]
        private string _editDescription = string.Empty;

        [ObservableProperty]
        private bool _isOrganizer;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<StageViewModel> _stages = new();

        [ObservableProperty]
        private ObservableCollection<TeamViewModel> _teams = new();

        [ObservableProperty]
        private ObservableCollection<SponsorContributionViewModel> _sponsorContributions = new();

        [ObservableProperty]
        private ObservableCollection<PrizeFundViewModel> _prizeFunds = new();

        // Команды
        public IAsyncRelayCommand SaveHakatonCommand { get; }
        public IAsyncRelayCommand AddStageCommand { get; }
        public IAsyncRelayCommand<StageViewModel> EditStageCommand { get; }
        public IAsyncRelayCommand<StageViewModel> DeleteStageCommand { get; }
        public IAsyncRelayCommand<StageViewModel> AddTaskCommand { get; }
        public IAsyncRelayCommand<TaskViewModel> EditTaskCommand { get; }
        public IAsyncRelayCommand<TaskViewModel> DeleteTaskCommand { get; }
        public IAsyncRelayCommand<TaskViewModel> AddCriteriaCommand { get; }
        public IAsyncRelayCommand<CriteriaViewModel> EditCriteriaCommand { get; }
        public IAsyncRelayCommand<CriteriaViewModel> DeleteCriteriaCommand { get; }


        [ObservableProperty] private bool _canRegister;
        [ObservableProperty] private string _registrationMessage = "";
        [ObservableProperty] private ObservableCollection<UserInviteDto> _availableUsers = new();
        [ObservableProperty] private UserInviteDto? _selectedUser;
        [ObservableProperty] private ObservableCollection<Role> _availableRoles = new();
        [ObservableProperty] private Role? _selectedRole;
        [ObservableProperty] private bool _showInvitePanel;

        public IAsyncRelayCommand RegisterCurrentUserCommand { get; }
        public IAsyncRelayCommand AddUserToHakatonCommand { get; }



        public HakatonDetailViewModel(IHakatonService hakatonService)
        {
            _hakatonService = hakatonService;

            SaveHakatonCommand = new AsyncRelayCommand(SaveHakatonAsync);
            AddStageCommand = new AsyncRelayCommand(AddStageAsync);
            EditStageCommand = new AsyncRelayCommand<StageViewModel>(EditStageAsync);
            DeleteStageCommand = new AsyncRelayCommand<StageViewModel>(DeleteStageAsync);
            AddTaskCommand = new AsyncRelayCommand<StageViewModel>(AddTaskAsync);
            EditTaskCommand = new AsyncRelayCommand<TaskViewModel>(EditTaskAsync);
            DeleteTaskCommand = new AsyncRelayCommand<TaskViewModel>(DeleteTaskAsync);
            AddCriteriaCommand = new AsyncRelayCommand<TaskViewModel>(AddCriteriaAsync);
            EditCriteriaCommand = new AsyncRelayCommand<CriteriaViewModel>(EditCriteriaAsync);
            DeleteCriteriaCommand = new AsyncRelayCommand<CriteriaViewModel>(DeleteCriteriaAsync);
            RegisterCurrentUserCommand = new AsyncRelayCommand(RegisterCurrentUserAsync);
            AddUserToHakatonCommand = new AsyncRelayCommand(AddUserToHakatonAsync);
        }

        partial void OnHakatonIdChanged(int value)
        {
            if (value > 0)
                _ = LoadDetailsAsync();
        }

        private async Task LoadDetailsAsync()
        {
            if (IsLoading || HakatonId <= 0) return;
            IsLoading = true;
            try
            {
                var details = await _hakatonService.GetHakatonDetailsAsync(HakatonId);
                Name = details.Name;
                Description = details.Description;
                EditName = details.Name;
                EditDescription = details.Description;
                IsOrganizer = details.CurrentUserRoleId == 3;   // 3 = организатор
                Stages = new ObservableCollection<StageViewModel>(details.Stages);
                Teams = new ObservableCollection<TeamViewModel>(details.Teams);
                SponsorContributions = new ObservableCollection<SponsorContributionViewModel>(details.SponsorContributions);
                PrizeFunds = new ObservableCollection<PrizeFundViewModel>(details.PrizeFunds);
                CanRegister = details.CurrentUserRoleId == 0 && AppState.CurrentUserId > 0;
                ShowInvitePanel = IsOrganizer;
                if (IsOrganizer)
                    await LoadAvailableUsersAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveHakatonAsync()
        {
            if (EditName != Name || EditDescription != Description)
            {
                await _hakatonService.UpdateHakatonAsync(HakatonId, EditName, EditDescription);
                Name = EditName;
                Description = EditDescription;
            }
        }

        private async Task AddStageAsync()
        {
            var dialog = new StageEditDialog();
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.ResultStage != null)
            {
                dialog.ResultStage.HakatonId = HakatonId;
                await _hakatonService.AddStageAsync(dialog.ResultStage);
                await LoadDetailsAsync();
            }
        }

        private async Task EditStageAsync(StageViewModel stageVm)
        {
            var stage = await _hakatonService.GetStageByIdAsync(stageVm.Id);
            if (stage == null) return;
            var dialog = new StageEditDialog(stage);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.ResultStage != null)
            {
                await _hakatonService.UpdateStageAsync(dialog.ResultStage);
                await LoadDetailsAsync();
            }
        }

        private async Task DeleteStageAsync(StageViewModel stageVm)
        {
            if (MessageBox.Show("Удалить этап? Все связанные задания и решения будут удалены.", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await _hakatonService.DeleteStageAsync(stageVm.Id);
                await LoadDetailsAsync();
            }
        }

        private async Task AddTaskAsync(StageViewModel stageVm)
        {
            var dialog = new TaskEditDialog(stageId: stageVm.Id);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.ResultTask != null)
            {
                await _hakatonService.AddTaskAsync(dialog.ResultTask);
                await LoadDetailsAsync();
            }
        }

        private async Task EditTaskAsync(TaskViewModel taskVm)
        {
            var task = await _hakatonService.GetTaskByIdAsync(taskVm.Id);
            if (task == null) return;
            var dialog = new TaskEditDialog(task);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.ResultTask != null)
            {
                await _hakatonService.UpdateTaskAsync(dialog.ResultTask);
                await LoadDetailsAsync();
            }
        }

        private async Task DeleteTaskAsync(TaskViewModel taskVm)
        {
            if (MessageBox.Show("Удалить задание? Все критерии и решения будут удалены.", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await _hakatonService.DeleteTaskAsync(taskVm.Id);
                await LoadDetailsAsync();
            }
        }

        private async Task AddCriteriaAsync(TaskViewModel taskVm)
        {
            var dialog = new CriteriaEditDialog(taskId: taskVm.Id);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.ResultCriteria != null)
            {
                await _hakatonService.AddCriteriaAsync(dialog.ResultCriteria);
                await LoadDetailsAsync();
            }
        }

        private async Task EditCriteriaAsync(CriteriaViewModel criteriaVm)
        {
            var criteria = await _hakatonService.GetCriteriaByIdAsync(criteriaVm.Id);
            if (criteria == null) return;
            var dialog = new CriteriaEditDialog(criteria);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true && dialog.ResultCriteria != null)
            {
                await _hakatonService.UpdateCriteriaAsync(dialog.ResultCriteria);
                await LoadDetailsAsync();
            }
        }

        private async Task DeleteCriteriaAsync(CriteriaViewModel criteriaVm)
        {
            if (MessageBox.Show("Удалить критерий?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await _hakatonService.DeleteCriteriaAsync(criteriaVm.Id);
                await LoadDetailsAsync();
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(MainWindowViewModel)));
        }

        private async Task RegisterCurrentUserAsync()
        {
            if (AppState.CurrentUserId == 0)
            {
                // Можно открыть окно логина через сообщение
                WeakReferenceMessenger.Default.Send(new OpenLoginMessage());
                return;
            }
            await _hakatonService.RegisterUserOnHakatonAsync(HakatonId, AppState.CurrentUserId, 1);
            RegistrationMessage = "Вы успешно зарегистрированы на хакатон!";
            await LoadDetailsAsync(); // обновить страницу – должен измениться CurrentUserRoleId
        }

        private async Task AddUserToHakatonAsync()
        {
            if (SelectedUser == null || SelectedRole == null) return;

            var user = SelectedUser;
            var role = SelectedRole;

            await _hakatonService.RegisterUserOnHakatonAsync(HakatonId, user.Id, role.Id);

            // Обновить список доступных пользователей (это сбросит SelectedUser и SelectedRole)
            await LoadAvailableUsersAsync();

            RegistrationMessage = $"Пользователь {user.FullName} добавлен с ролью {role.Name}";
        }

        private async Task LoadAvailableUsersAsync()
        {
            var users = await _hakatonService.GetAvailableUsersForHakatonAsync(HakatonId);
            AvailableUsers = new ObservableCollection<UserInviteDto>(users);
            var roles = await _hakatonService.GetAllRolesAsync();
            AvailableRoles = new ObservableCollection<Role>(roles);
            SelectedRole = AvailableRoles.FirstOrDefault(r => r.Id == 1); // участник по умолчанию
        }

    }

    // Вспомогательные ViewModel (без изменений, но для полноты оставлены)
    public partial class StageViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string? _description;
        [ObservableProperty] private DateTime? _startDate;
        [ObservableProperty] private DateTime? _endDate;
        [ObservableProperty] private int? _orderNumber;
        [ObservableProperty] private int? _locationId;
        [ObservableProperty] private int? _stageTypeId;
        [ObservableProperty] private System.Collections.Generic.List<TaskViewModel> _tasks = new();
    }

    public partial class TaskViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string? _description;
        [ObservableProperty] private bool _isSolutionsPublic;
        [ObservableProperty] private System.Collections.Generic.List<CriteriaViewModel> _criteria = new();
    }

    public partial class CriteriaViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string? _name;
        [ObservableProperty] private string? _description;
        [ObservableProperty] private decimal? _maxMark;
    }

    public partial class TeamViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string? _name;
        [ObservableProperty] private System.Collections.Generic.List<string> _members = new();
        public string MembersString => string.Join(", ", Members);
    }

    public partial class SponsorContributionViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string? _sponsorName;
        [ObservableProperty] private decimal? _money;
        [ObservableProperty] private string? _description;
    }

    public partial class PrizeFundViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string? _nominationName;
        [ObservableProperty] private int _place;
        [ObservableProperty] private decimal? _amount;
        [ObservableProperty] private string? _winnerTeamName;
    }
}