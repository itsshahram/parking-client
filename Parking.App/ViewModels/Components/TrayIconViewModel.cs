using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Parking.App.ViewModels.Components;

public partial class TrayIconViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<System.Windows.Controls.MenuItem> _trayMenuItems =
    [
        new System.Windows.Controls.MenuItem { Header = "Home", Tag = "tray_home" },
        new System.Windows.Controls.MenuItem { Header = "Close", Tag = "tray_close" },
    ];
    [ObservableProperty]
    private ImageSource iconSource;
}
