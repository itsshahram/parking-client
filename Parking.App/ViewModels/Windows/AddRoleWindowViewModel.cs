using System.ComponentModel;

namespace Parking.App.ViewModels.Windows;

public class AddRoleWindowViewModel : INotifyPropertyChanged
{

    private string _name;
    private string _faName;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
    public string FaName
    {
        get => _faName;
        set
        {
            _faName = value;
            OnPropertyChanged(nameof(FaName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string prop) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
