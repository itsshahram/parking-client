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

    public (ModifierKeys modifiers, Key key, bool allowSingleKey) GetDefaultShortcut(HotKeyActionType action)
    {
        return action switch
        {
            HotKeyActionType.CreateTicket => (ModifierKeys.None, Key.Enter, true),
            HotKeyActionType.SearchBarcode => (ModifierKeys.None, Key.Enter, false),
            HotKeyActionType.CashPayment => (ModifierKeys.None, Key.F1, true),
            HotKeyActionType.PaymentWithSpace => (ModifierKeys.None, Key.Space, true),
            HotKeyActionType.PrintReceipt => (ModifierKeys.None, Key.F3, true),
            HotKeyActionType.LostCard => (ModifierKeys.None, Key.F12, true),
            HotKeyActionType.MaiPageResetForm => (ModifierKeys.None, Key.F5, true),
            _ => (ModifierKeys.None, Key.None, true)
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
                var (modifiers, key, allowSingleKey) = GetDefaultShortcut(action);

                HotKeyConfigs.Add(new HotKeyConfig
                {
                    Name = action.ToString(),
                    AllowSingleKey = allowSingleKey,
                    Type = action,
                    Key = key,
                    Modifiers = modifiers
                });
            }
        }
    }
}
