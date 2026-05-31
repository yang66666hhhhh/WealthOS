using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WealthOS.Application.Interfaces;
using WealthOS.Presentation.Services;

namespace WealthOS.Presentation.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IDbContext _dbContext;
    private readonly LocalizationService _localization;

    [ObservableProperty]
    private string _dbPath = string.Empty;

    [ObservableProperty]
    private string _dbSize = string.Empty;

    [ObservableProperty]
    private string _lastBackupTime = "从未备份";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel(IDbContext dbContext, LocalizationService localization)
    {
        _dbContext = dbContext;
        _localization = localization;
        LoadInfo();
    }

    private void LoadInfo()
    {
        DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WealthOS", "wealthos.db");

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
        try
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"WealthOS_Backup_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = ".db",
                Filter = "数据库文件|*.db"
            };

            if (dialog.ShowDialog() == true)
            {
                File.Copy(DbPath, dialog.FileName, true);
                LastBackupTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                StatusMessage = "备份成功";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"备份失败：{ex.Message}";
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
            Filter = "数据库文件|*.db"
        };

        if (dialog.ShowDialog() == true)
        {
            IsBusy = true;
            try
            {
                File.Copy(dialog.FileName, DbPath, true);
                StatusMessage = "恢复成功，重启应用后生效";
            }
            catch (Exception ex)
            {
                StatusMessage = $"恢复失败：{ex.Message}";
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
        StatusMessage = _localization.IsChinese ? "已切换到中文" : "Switched to English";
    }
}
