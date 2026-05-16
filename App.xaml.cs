using HakatonApplication.Context;
using HakatonApplication.Service;
using HakatonApplication.View;
using HakatonApplication.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace HakatonApplication
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            services.AddDbContext<HakatonDbContext>(options =>
                options.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=1111"));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IHakatonService, HakatonService>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<MainWindow>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<HakatonDetailViewModel>();


            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}