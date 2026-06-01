using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using WealthOS.Application.Interfaces;
using WealthOS.Presentation.Services;

namespace WealthOS.Presentation.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IDbContext _dbContext;
    private readonly LocalizationService _localization;

    [ObservableProperty]
    private string _dbPath = string.Empty;

    [ObservableProperty]
    private string _dbSize = string.Empty;

    [ObservableProperty]
    private string _lastBackupTime = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public bool IsDarkMode => _localization.IsDarkMode;

    public SettingsViewModel(IDbContext dbContext, LocalizationService localization)
    {
        _dbContext = dbContext;
        _localization = localization;
        LoadInfo();
    }

    public override IRelayCommand? RefreshCommand => null;

    private void LoadInfo()
    {
        DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WealthOS", "wealthos.db");

        LastBackupTime = GetResourceString("Settings.NeverBackup");

        if (File.Exists(DbPath))
        {
            var info = new FileInfo(DbPath);
            DbSize = info.Length switch
            {
                < 1024 => $"{info.Length} B",
                < 1024 * 1024 => $"{info.Length / 1024:N1} KB",
                _ => $"{info.Length / (1024 * 1024):N1} MB"
            };
        }
    }

    [RelayCommand]
    private async Task BackupAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"WealthOS_Backup_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = ".db",
                Filter = GetResourceString("FileDialog.DbFilter")
            };

            if (dialog.ShowDialog() == true)
            {
                if (!File.Exists(DbPath))
                {
                    StatusMessage = GetResourceString("Settings.DatabaseNotFound");
                    return;
                }
                await Task.Run(() => File.Copy(DbPath, dialog.FileName, true));
                LastBackupTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                StatusMessage = GetResourceString("Settings.BackupSuccess");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"{GetResourceString("Settings.BackupFailed")}：{ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RestoreAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = GetResourceString("FileDialog.DbFilter")
        };

        if (dialog.ShowDialog() == true)
        {
            IsBusy = true;
            ClearError();
            try
            {
                await Task.Run(() =>
                {
                    using var conn = new SqliteConnection($"Data Source={dialog.FileName}");
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "PRAGMA integrity_check;";
                    var result = cmd.ExecuteScalar()?.ToString();
                    if (result != "ok")
                        throw new InvalidOperationException("Database integrity check failed.");
                });
                await Task.Run(() => File.Copy(dialog.FileName, DbPath, true));
                StatusMessage = GetResourceString("Settings.RestoreSuccess") + " " + GetResourceString("Settings.RestartRequired");
            }
            catch (Exception ex)
            {
                StatusMessage = $"{GetResourceString("Settings.InvalidBackupFile")}：{ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        _localization.ToggleLanguage();
        StatusMessage = _localization.IsChinese
            ? GetResourceString("Settings.SwitchedToChinese")
            : GetResourceString("Settings.SwitchedToEnglish");
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        _localization.ToggleTheme();
        OnPropertyChanged(nameof(IsDarkMode));
        StatusMessage = _localization.IsDarkMode
            ? GetResourceString("Settings.SwitchedToDark")
            : GetResourceString("Settings.SwitchedToLight");
    }
}
