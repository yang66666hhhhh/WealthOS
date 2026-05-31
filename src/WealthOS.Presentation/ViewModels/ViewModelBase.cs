using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WealthOS.Presentation.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [RelayCommand]
    protected void ClearError()
    {
        ErrorMessage = null;
        HasError = false;
    }

    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    protected void SetError(Exception ex)
    {
        ErrorMessage = ex.Message;
        HasError = true;
    }
}
