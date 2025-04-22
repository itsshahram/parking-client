using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.ViewModels.Windows;

public class ChangePasswordWindowViewModel : INotifyPropertyChanged
{
    private Guid userId;
    public Guid UserId
    {
        get => userId;
        set
        {
            userId = value;
            OnPropertyChanged(nameof(UserId));
        }
    }
    private string oldPassword;
    public string OldPassword
    {
        get => oldPassword;
        set
        {
            oldPassword = value;
            OnPropertyChanged(nameof(OldPassword));
        }
    }
    private string newPassword;
    public string NewPassword
    {
        get => newPassword;
        set
        {
            newPassword = value;
            OnPropertyChanged(nameof(NewPassword));
        }
    }
    private string confirmPassword;
    public string ConfirmPassword
    {
        get => confirmPassword;
        set
        {
            confirmPassword = value;
            OnPropertyChanged(nameof(ConfirmPassword));
        }
    }
    private string userFullName;
    public string UserFullName
    {
        get => userFullName;
        set
        {
            userFullName = value;
            OnPropertyChanged(nameof(UserFullName));
        }
    }
    private string userName;
    public string UserName
    {
        get => userName;
        set
        {
            userName = value;
            OnPropertyChanged(nameof(UserName));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
