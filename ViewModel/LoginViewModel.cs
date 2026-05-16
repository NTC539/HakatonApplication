using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HakatonApplication.Message;
using HakatonApplication.Service;
using HakatonApplication.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HakatonApplication.ViewModel

{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IHakatonService _hakatonService;

        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _password = "";
        [ObservableProperty] private string _errorMessage = "";
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _isRegisterMode;

        [ObservableProperty] private string _regFirstName = "";
        [ObservableProperty] private string _regLastName = "";
        [ObservableProperty] private string _regPatronymic = "";
        [ObservableProperty] private string _regEmail = "";
        [ObservableProperty] private string _regPhone = "";
        [ObservableProperty] private string _regPassword = "";
        [ObservableProperty] private string _regConfirmPassword = "";

        public IAsyncRelayCommand LoginCommand { get; }
        public IAsyncRelayCommand RegisterCommand { get; }
        public RelayCommand SwitchModeCommand { get; }

        // Событие для закрытия окна входа и открытия главного окна
        public event System.Action? LoginSucceeded;

        public LoginViewModel(IAuthService authService, IHakatonService hakatonService)
        {
            _authService = authService;
            _hakatonService = hakatonService;
            LoginCommand = new AsyncRelayCommand(LoginAsync);
            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
            SwitchModeCommand = new RelayCommand(SwitchMode);
        }

        private void SwitchMode()
        {
            IsRegisterMode = !IsRegisterMode;
            ErrorMessage = "";
        }

        private async Task LoginAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var userId = await _authService.LoginAsync(Email, Password);
                if (userId.HasValue)
                {
                    AppState.CurrentUserId = userId.Value;
                    var userInfo = await _authService.GetUserInfoAsync(userId.Value);
                    WeakReferenceMessenger.Default.Send(new LoginSuccessMessage
                    {
                        UserId = userId.Value,
                        FirstName = userInfo.FirstName,
                        LastName = userInfo.LastName,
                        IsAdmin = userInfo.IsAdmin
                    });
                    // Закрыть окно логина
                    if (Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault() is Window loginWindow)
                        loginWindow.Close();
                    return;
                }
                else
                {
                    ErrorMessage = "Неверный email или пароль.";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RegisterAsync()
        {
            if (IsLoading) return;
            if (RegPassword != RegConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают.";
                return;
            }

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var dto = new DTO.RegisterDto
                {
                    Email = RegEmail,
                    Phone = RegPhone,
                    Password = RegPassword,
                    FirstName = RegFirstName,
                    LastName = RegLastName,
                    Patronymic = RegPatronymic
                };
                var userId = await _authService.RegisterAsync(dto);
                if (userId.HasValue)
                {
                    // Автоматический вход после регистрации
                    var loginResult = await _authService.LoginAsync(RegEmail, RegPassword);
                    if (loginResult.HasValue)
                    {
                        AppState.CurrentUserId = loginResult.Value;
                        var userInfo = await _authService.GetUserInfoAsync(loginResult.Value);
                        WeakReferenceMessenger.Default.Send(new LoginSuccessMessage
                        {
                            UserId = loginResult.Value,
                            FirstName = userInfo.FirstName,
                            LastName = userInfo.LastName,
                            IsAdmin = userInfo.IsAdmin
                        });
                        // Закрыть окно логина
                        if (Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault() is Window loginWindow)
                            loginWindow.Close();
                        return;
                    }
                }
                else
                {
                    ErrorMessage = "Ошибка регистрации. Возможно, email уже используется.";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
