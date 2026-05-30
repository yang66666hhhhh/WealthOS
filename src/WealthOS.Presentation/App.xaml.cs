using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WealthOS.Application.Interfaces;
using WealthOS.Application.Services;
using WealthOS.Infrastructure.Data;
using WealthOS.Infrastructure.Repositories;
using WealthOS.Presentation.Services;
using WealthOS.Presentation.ViewModels;

namespace WealthOS.Presentation;

public partial class App : System.Windows.Application
{
    private readonly IServiceProvider _services;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WealthOS",
            "wealthos.db");

        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        services.AddSingleton<IDbContext>(new SqliteDbContext($"Data Source={dbPath}"));
        services.AddSingleton<DatabaseInitializer>();

        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<IAssetRepository, AssetRepository>();
        services.AddSingleton<ILiabilityRepository, LiabilityRepository>();
        services.AddSingleton<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IGoalRepository, GoalRepository>();
        services.AddSingleton<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<INetWorthRepository, NetWorthRepository>();

        services.AddSingleton<DashboardService>();
        services.AddSingleton<AssetService>();
        services.AddSingleton<LiabilityService>();
        services.AddSingleton<TransactionService>();
        services.AddSingleton<GoalService>();

        services.AddSingleton<NavigationService>();

        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AssetsViewModel>();
        services.AddTransient<LiabilitiesViewModel>();
        services.AddTransient<TransactionsViewModel>();
        services.AddTransient<GoalsViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var dbInitializer = _services.GetRequiredService<DatabaseInitializer>();
        await dbInitializer.InitializeAsync();

        var mainWindow = _services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
