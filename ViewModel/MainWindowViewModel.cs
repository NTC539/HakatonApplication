using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HakatonApplication.DTO;
using HakatonApplication.Message;
using HakatonApplication.Service;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HakatonApplication.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IHakatonService _hakatonService;

        [ObservableProperty]
        private ObservableCollection<HakatonListItemDto> _hakatons = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private object? _currentViewModel;  // текущая страница

        public MainWindowViewModel(IHakatonService hakatonService)
        {
            _hakatonService = hakatonService;
            CurrentViewModel = this; // стартовая страница – список

            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, OnNavigationMessage);
        }

        private void OnNavigationMessage(object recipient, NavigationMessage message)
        {
            if (message.ViewModelType == typeof(HakatonDetailViewModel) && message.Id.HasValue)
            {
                var detailVm = new HakatonDetailViewModel(message.Id.Value, _hakatonService);
                CurrentViewModel = detailVm;
            }
            else if (message.ViewModelType == typeof(MainWindowViewModel))
            {
                CurrentViewModel = this;
                _ = LoadHakatonsAsync(); // обновить список при возврате
            }
        }

        [RelayCommand]
        private async Task LoadHakatonsAsync() => await LoadDataAsync(null);

        [RelayCommand]
        private async Task SearchAsync() => await LoadDataAsync(SearchText);

        private async Task LoadDataAsync(string? searchText)
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                var items = await _hakatonService.GetAllHakatonsAsync(searchText);
                Hakatons = new ObservableCollection<HakatonListItemDto>(items);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        private void OpenHakaton(int id)
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(HakatonDetailViewModel), id));
        }
    }
}