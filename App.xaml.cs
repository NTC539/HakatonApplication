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
            services.AddScoped<IHakatonService, HakatonService>();
            services.AddTransient<MainWindowViewModel>(); // не Singleton

            var provider = services.BuildServiceProvider();

            var viewModel = provider.GetRequiredService<MainWindowViewModel>();
            var mainWindow = new MainWindow(viewModel);
            mainWindow.Show();
        }
    }

}
