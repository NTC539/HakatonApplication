using HakatonApplication.Context;
using HakatonApplication.Service;
using HakatonApplication.View;
using HakatonApplication.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace HakatonApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            services.AddDbContext<HakatonDbContext>(options =>
                options.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=1111"));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IHakatonService, HakatonService>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<LoginWindow>();
            services.AddSingleton<MainWindowViewModel>(); 
            services.AddTransient<MainWindow>();

            var provider = services.BuildServiceProvider();

            var loginWindow = provider.GetRequiredService<LoginWindow>();
            bool? result = loginWindow.ShowDialog();
            if (result == true)
            {
                var mainWindow = provider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }

}
