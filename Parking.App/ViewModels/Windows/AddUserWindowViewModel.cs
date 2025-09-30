using Parking.Domain.Entities.User;
using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Parking.App.ViewModels.Windows;

public class AddUserWindowViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();

    private string _firstName;
    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged(nameof(FirstName));
            ValidateRequired(nameof(FirstName), value);
        }
    }

    private string _lastName;
    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged(nameof(LastName));
            ValidateRequired(nameof(LastName), value);
        }
    }

    private string _username;
    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged(nameof(Username));
            ValidateRequired(nameof(Username), value);
            ValidateEmail(nameof(Username), value);
        }
    }

    private string _password;
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
            ValidateRequired(nameof(Password), value);
        }
    }

    private string _role;
    public string Role
    {
        get => _role;
        set
        {
            _role = value;
            OnPropertyChanged(nameof(Role));
            ValidateRequired(nameof(Role), value);
        }
    }

    private List<ApplicationRole> _roles;
    public List<ApplicationRole> Roles
    {
        get => _roles;
        set
        {
            _roles = value;
            OnPropertyChanged(nameof(Roles));
            ValidateRequired(nameof(Role), Role); 
        }
    }
    // --- Validation helpers ---
    private void ValidateRequired(string propertyName, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            AddError(propertyName, "این فیلد الزامی است.");
        else
            ClearErrors(propertyName);
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();

        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ValidateEmail(string propertyName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(value))
            {
                AddError(propertyName, "ایمیل وارد شده معتبر نیست");
                return;
            }
        }
        ClearErrors(propertyName);
    }
    public bool HasErrors => _errors.Count > 0;
    public IEnumerable GetErrors(string propertyName) =>
        _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string prop) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
}
