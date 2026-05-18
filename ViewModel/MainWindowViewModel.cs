using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HakatonApplication.DTO;
using HakatonApplication.Message;
using HakatonApplication.Models;
using HakatonApplication.Service;
using HakatonApplication.View;
using HakatonApplication.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace HakatonApplication.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IHakatonService _hakatonService;
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty] private ObservableCollection<HakatonListItemDto> _hakatons = new();
        [ObservableProperty] private string _searchText = string.Empty;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private object? _currentViewModel;
        [ObservableProperty] private bool _isCurrentUserAdmin;

        // Информация о текущем пользователе
        [ObservableProperty] private bool _isAuthenticated;
        [ObservableProperty] private string _userName = "Гость";

        public IAsyncRelayCommand LoadHakatonsCommand { get; }
        public IAsyncRelayCommand SearchCommand { get; }
        public IRelayCommand OpenLoginCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        public MainWindowViewModel(IHakatonService hakatonService, IAuthService authService, IServiceProvider serviceProvider)
        {
            _hakatonService = hakatonService;
            _authService = authService;
            _serviceProvider = serviceProvider;

            LoadHakatonsCommand = new AsyncRelayCommand(LoadHakatonsAsync);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
            OpenLoginCommand = new RelayCommand(OpenLogin);
            LogoutCommand = new RelayCommand(Logout);

            CurrentViewModel = this; // стартовая страница – список хакатонов

            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, OnNavigationMessage);
            WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this, OnLoginSuccess);
            WeakReferenceMessenger.Default.Register<OpenLoginMessage>(this, (r, m) => OpenLogin());
        }

        private async Task LoadHakatonsAsync() => await LoadDataAsync(null);
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

        private void OpenLogin()
        {
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            loginWindow.Owner = Application.Current.MainWindow;
            loginWindow.ShowDialog();
        }

        private void Logout()
        {
            AppState.CurrentUserId = 0;
            IsAuthenticated = false;
            UserName = "Гость";
            // Вернуться на главную страницу
            CurrentViewModel = this;
        }

        private void OnLoginSuccess(object recipient, LoginSuccessMessage message)
        {
            IsAuthenticated = true;
            UserName = $"{message.FirstName} {message.LastName}".Trim();
            IsCurrentUserAdmin = message.IsAdmin;
            AppState.CurrentUserId = message.UserId;
            _ = LoadHakatonsAsync(); 
        }

        private void OnNavigationMessage(object recipient, NavigationMessage message)
        {
            if (message.ViewModelType == typeof(HakatonDetailViewModel) && message.Id.HasValue)
            {
                var detailVm = _serviceProvider.GetRequiredService<HakatonDetailViewModel>();
                detailVm.HakatonId = message.Id.Value;
                CurrentViewModel = detailVm;
            }
            else if (message.ViewModelType == typeof(MainWindowViewModel))
            {
                CurrentViewModel = this;
                _ = LoadHakatonsAsync();
            }
        }

        [RelayCommand]
        private async Task CreateHakatonAsync()
        {
            if (AppState.CurrentUserId == 0)
            {
                MessageBox.Show("Необходимо войти для создания хакатона.");
                OpenLogin();
                return;
            }
            var newId = await _hakatonService.CreateEmptyHakatonAsync(AppState.CurrentUserId);
            var detailVm = _serviceProvider.GetRequiredService<HakatonDetailViewModel>();
            detailVm.HakatonId = newId;
            CurrentViewModel = detailVm;
        }
    }
    public class OpenLoginMessage { }
}
