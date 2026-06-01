using System.IO;
using System.Windows;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using WealthOS.Application.Interfaces;
using WealthOS.Application.Services;
using WealthOS.Infrastructure.Data;
using WealthOS.Infrastructure.Repositories;
using WealthOS.Presentation.Services;
using WealthOS.Presentation.ViewModels;
using WealthOS.Presentation.Views;

namespace WealthOS.Presentation;

public partial class App : System.Windows.Application
{
    private Mutex? _mutex;
    private readonly IServiceProvider _services;

    public App()
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new NullableGuidTypeHandler());
        SqlMapper.AddTypeHandler(new DecimalTypeHandler());
        
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
        services.AddSingleton<IBudgetRepository, BudgetRepository>();
        services.AddSingleton<IInvestmentHoldingRepository, InvestmentHoldingRepository>();

        services.AddSingleton<DashboardService>();
        services.AddSingleton<AssetService>();
        services.AddSingleton<LiabilityService>();
        services.AddSingleton<TransactionService>();
        services.AddSingleton<GoalService>();
        services.AddSingleton<AccountService>();
        services.AddSingleton<CategoryService>();
        services.AddSingleton<NetWorthService>();
        services.AddSingleton<InvestmentService>();
        services.AddSingleton<ReportService>();
        services.AddSingleton<BudgetService>();
        services.AddSingleton<SeedDataService>();

        services.AddSingleton<NavigationService>();
        services.AddSingleton<LocalizationService>();

        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<AssetsViewModel>();
        services.AddSingleton<LiabilitiesViewModel>();
        services.AddSingleton<TransactionsViewModel>();
        services.AddSingleton<GoalsViewModel>();
        services.AddSingleton<AccountsViewModel>();
        services.AddSingleton<InvestmentsViewModel>();
        services.AddSingleton<FixedAssetsViewModel>();
        services.AddSingleton<AnalyticsViewModel>();
        services.AddSingleton<TimelineViewModel>();
        services.AddSingleton<ReportsViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<BudgetsViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
    }

    private static string GetString(string key)
    {
        if (Current.TryFindResource(key) is string value)
            return value;
        return key;
    }

    private static void ShowErrorAndShutdown(string title, string message, string? stackTrace = null)
    {
        System.Diagnostics.Debug.WriteLine($"{title}: {message}");
        var errorWindow = new StartupErrorWindow(title, message, stackTrace);
        errorWindow.ShowDialog();
        Current.Shutdown();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        bool isNewInstance;
        try
        {
            _mutex = new Mutex(true, "WealthOS_SingleInstance", out isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show(GetString("App.AlreadyRunning"), "WealthOS", MessageBoxButton.OK, MessageBoxImage.Information);
                Current.Shutdown();
                return;
            }
        }
        catch (AbandonedMutexException)
        {
            // Previous instance crashed - we can proceed
        }

        base.OnStartup(e);

        try
        {
            var dbInitializer = _services.GetRequiredService<DatabaseInitializer>();
            await dbInitializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            ShowErrorAndShutdown(GetString("Startup.DbInitFailed"), ex.Message, ex.StackTrace);
            return;
        }

        try
        {
            var seedDataService = _services.GetRequiredService<SeedDataService>();
            await seedDataService.SeedAllAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"初始数据填充失败: {ex.Message}");
        }

        try
        {
            var netWorthService = _services.GetRequiredService<NetWorthService>();
            await netWorthService.SaveSnapshotIfNotExistsTodayAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"净资产快照失败: {ex.Message}");
        }

        try
        {
            var mainWindow = _services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            ShowErrorAndShutdown(GetString("Startup.WindowCreateFailed"), ex.Message, ex.StackTrace);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
