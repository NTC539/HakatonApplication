using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HakatonApplication.Message;
using HakatonApplication.Models;
using HakatonApplication.Service;
using HakatonApplication.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        private ObservableCollection<StageViewModel> _stages = new();

        [ObservableProperty]
        private ObservableCollection<TeamViewModel> _teams = new();

        [ObservableProperty]
        private ObservableCollection<SponsorContributionViewModel> _sponsorContributions = new();

        [ObservableProperty]
        private ObservableCollection<PrizeFundViewModel> _prizeFunds = new();

        [ObservableProperty]
        private bool _isLoading;

        public IAsyncRelayCommand LoadDetailsCommand { get; }

        [RelayCommand]
        private void GoBack()
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(MainWindowViewModel)));
        }

        public HakatonDetailViewModel(IHakatonService hakatonService)
        {
            _hakatonService = hakatonService;
            LoadDetailsCommand = new AsyncRelayCommand(LoadDetailsAsync);
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
                Stages = new ObservableCollection<StageViewModel>(details.Stages);
                Teams = new ObservableCollection<TeamViewModel>(details.Teams);
                SponsorContributions = new ObservableCollection<SponsorContributionViewModel>(details.SponsorContributions);
                PrizeFunds = new ObservableCollection<PrizeFundViewModel>(details.PrizeFunds);
            }
            finally
            {
                IsLoading = false;
            }
        }

    }

    // Вспомогальные ViewModel для вложенных данных
    public partial class StageViewModel : ObservableObject
    {
        [ObservableProperty] private string? _description;
        [ObservableProperty] private DateTime? _startDate;
        [ObservableProperty] private DateTime? _endDate;
        [ObservableProperty] private List<TaskViewModel> _tasks = new();
    }

    public partial class TaskViewModel : ObservableObject
    {
        [ObservableProperty] private string? _description;
        [ObservableProperty] private bool _isSolutionsPublic;
        [ObservableProperty] private List<CriteriaViewModel> _criteria = new();
    }

    public partial class CriteriaViewModel : ObservableObject
    {
        [ObservableProperty] private string? _name;
        [ObservableProperty] private string? _description;
        [ObservableProperty] private decimal? _maxMark;
    }

    public partial class TeamViewModel : ObservableObject
    {
        [ObservableProperty] private string? _name;
        [ObservableProperty] private List<string> _members = new();
        public string MembersString => string.Join(", ", Members);
    }

    public partial class SponsorContributionViewModel : ObservableObject
    {
        [ObservableProperty] private string? _sponsorName;
        [ObservableProperty] private decimal? _money;
        [ObservableProperty] private string? _description;
    }

    public partial class PrizeFundViewModel : ObservableObject
    {
        [ObservableProperty] private string? _nominationName;
        [ObservableProperty] private int _place;
        [ObservableProperty] private decimal? _amount;
        [ObservableProperty] private string? _winnerTeamName;
    }
}