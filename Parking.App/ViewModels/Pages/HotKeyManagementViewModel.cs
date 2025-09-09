namespace Parking.App.ViewModels.Pages;

public partial class HotKeyManagementViewModel : ObservableObject
{
    public ObservableCollection<HotKeyConfig> HotKeyConfigs { get; set; } = new();

    private readonly string FilePath = "hotkeys.json";

    public HotKeyManagementViewModel()
    {
        Load();
        EnsureAllActionsPresent();
    }

    private string GetFullPath() => Path.Combine(Constants.AppDataFolder, FilePath);

    public void Load()
    {
        string path = GetFullPath();
        if (!File.Exists(path))
            return;

        var json = File.ReadAllText(path);
        var loaded = JsonConvert.DeserializeObject<List<HotKeyConfig>>(json);
        if (loaded != null)
        {
            HotKeyConfigs.Clear();
            foreach (var hk in loaded)
                HotKeyConfigs.Add(hk);
        }
    }

    public void Save()
    {
        string path = GetFullPath();
        var json = JsonConvert.SerializeObject(HotKeyConfigs, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    private (ModifierKeys modifiers, Key key) GetDefaultShortcut(HotKeyActionType action)
    {
        return action switch
        {
            HotKeyActionType.CreateTicket => (ModifierKeys.None, Key.Enter),
            HotKeyActionType.SearchBarcode => (ModifierKeys.None, Key.Enter),
            HotKeyActionType.CashPayment => (ModifierKeys.None, Key.F1),
            HotKeyActionType.PaymentWithSpace => (ModifierKeys.None, Key.Space),
            HotKeyActionType.PrintReceipt => (ModifierKeys.None, Key.F3),
            HotKeyActionType.LostCard => (ModifierKeys.None, Key.F12),
            HotKeyActionType.MaiPageResetForm => (ModifierKeys.None, Key.F5),
            _ => (ModifierKeys.None, Key.None)
        };
    }
    public HotKeyConfig GetHotKey(HotKeyActionType action)
        => HotKeyConfigs.FirstOrDefault(h => h.Type == action);
    public string FormatHotkey(HotKeyConfig config)
    {
        if (config == null || config.Key == Key.None)
            return string.Empty;

        var parts = new List<string>();

        if ((config.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            parts.Add("Ctrl");
        if ((config.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            parts.Add("Alt");
        if ((config.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            parts.Add("Shift");
        if ((config.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
            parts.Add("Win");

        parts.Add(config.Key.ToString());
        return string.Join(" + ", parts);
    }

    public void EnsureAllActionsPresent()
    {
        foreach (HotKeyActionType action in Enum.GetValues(typeof(HotKeyActionType)))
        {
            if (!HotKeyConfigs.Any(h => h.Type == action))
            {
                var (modifiers, key) = GetDefaultShortcut(action);

                HotKeyConfigs.Add(new HotKeyConfig
                {
                    Name = action.ToString(),
                    Type = action,
                    Key = key,
                    Modifiers = modifiers
                });
            }
        }
    }
}
