using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HakatonApplication.DTO;
using HakatonApplication.Message;
using HakatonApplication.Service;
using HakatonApplication.View;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace HakatonApplication.ViewModel
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IHakatonService _hakatonService;

        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _errorMessage = "";
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private bool _isRegisterMode;

        [ObservableProperty] private string _regFirstName = "";
        [ObservableProperty] private string _regLastName = "";
        [ObservableProperty] private string _regPatronymic = "";
        [ObservableProperty] private string _regEmail = "";
        [ObservableProperty] private string _regPhone = "";

        public LoginViewModel(IAuthService authService, IHakatonService hakatonService)
        {
            _authService = authService;
            _hakatonService = hakatonService;
        }

        public void SwitchMode()
        {
            IsRegisterMode = !IsRegisterMode;
            ErrorMessage = "";
        }

        public async Task LoginWithPasswordAsync(string password)
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                if (!IsValidEmail(Email))
                {
                    ErrorMessage = "Введите корректный email (например, user@example.com).";
                    return;
                }
                if (string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage = "Пароль не может быть пустым.";
                    return;
                }

                var userId = await _authService.LoginAsync(Email, password);
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

        public async Task RegisterWithPasswordsAsync(string password, string confirmPassword)
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                if (string.IsNullOrWhiteSpace(RegLastName))
                {
                    ErrorMessage = "Фамилия обязательна для заполнения.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(RegFirstName))
                {
                    ErrorMessage = "Имя обязательно для заполнения.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(RegEmail))
                {
                    ErrorMessage = "Email обязателен для заполнения.";
                    return;
                }

                if (!IsValidEmail(RegEmail))
                {
                    ErrorMessage = "Введите корректный email (например, user@example.com).";
                    return;
                }

                if (!string.IsNullOrWhiteSpace(RegPhone) && !IsValidPhone(RegPhone))
                {
                    ErrorMessage = "Телефон должен содержать только цифры, пробелы, знак + и дефисы. Пример: +7 123 456-78-90";
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage = "Пароль не может быть пустым.";
                    return;
                }
                if (password.Length < 6)
                {
                    ErrorMessage = "Пароль должен содержать не менее 6 символов.";
                    return;
                }
                if (password != confirmPassword)
                {
                    ErrorMessage = "Пароли не совпадают.";
                    return;
                }

                if (RegLastName.Length > 50)
                {
                    ErrorMessage = "Фамилия не должна превышать 50 символов.";
                    return;
                }
                if (RegFirstName.Length > 50)
                {
                    ErrorMessage = "Имя не должно превышать 50 символов.";
                    return;
                }
                if (RegPatronymic.Length > 50)
                {
                    ErrorMessage = "Отчество не должно превышать 50 символов.";
                    return;
                }

                var dto = new RegisterDto
                {
                    Email = RegEmail,
                    Phone = string.IsNullOrWhiteSpace(RegPhone) ? null : RegPhone,
                    Password = password,
                    FirstName = RegFirstName.Trim(),
                    LastName = RegLastName.Trim(),
                    Patronymic = string.IsNullOrWhiteSpace(RegPatronymic) ? null : RegPatronymic.Trim()
                };

                var userId = await _authService.RegisterAsync(dto);
                if (userId.HasValue)
                {
                    var loginResult = await _authService.LoginAsync(RegEmail, password);
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
                        if (Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault() is Window loginWindow)
                            loginWindow.Close();
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

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; 

            var cleaned = phone.Trim();
            var regex = new Regex(@"^\+?[\d\s\-\(\)]{7,20}$");
            if (!regex.IsMatch(cleaned))
                return false;

            var digits = new string(cleaned.Where(char.IsDigit).ToArray());
            return digits.Length >= 7 && digits.Length <= 15;
        }
    }
}