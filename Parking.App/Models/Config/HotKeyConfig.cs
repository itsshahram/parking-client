using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models;

public class HotKeyConfig
{
    public string Name { get; set; }
    public HotKeyActionType Type { get; set; }
    public Key Key { get; set; }
    public ModifierKeys Modifiers { get; set; }

    public string Shortcut
    {
        get => Modifiers == ModifierKeys.None ? $"{Key}" : $"{Modifiers} + {Key}";
        set
        {
            ParseShortcut(value, out var mods, out var key);
            Modifiers = mods;
            Key = key;
        }
    }


    private void ParseShortcut(string shortcut, out ModifierKeys mods, out Key key)
    {
        mods = ModifierKeys.None;
        key = Key.None;

        if (string.IsNullOrWhiteSpace(shortcut)) return;

        string[] parts = shortcut.Split('+');
        foreach (var part in parts)
        {
            string trimmed = part.Trim();
            if (Enum.TryParse(trimmed, true, out ModifierKeys m))
                mods |= m;
            else if (Enum.TryParse(trimmed, true, out Key k))
                key = k;
        }
    }

    public override string ToString() => Shortcut;
}

public enum HotKeyActionType
{
    [Display(Name = "ثبت قبض")]
    CreateTicket,

    [Display(Name = "جستجو قبض")]
    SearchBarcode,

    [Display(Name = "پرداخت نقدی")]
    CashPayment,

    [Display(Name = "پرداخت  با پوز")]
    PaymentWithSpace,

    [Display(Name = "چاپ رسید")]
    PrintReceipt,

    [Display(Name = "مفقود کردن کارت")]
    LostCard,

    [Display(Name = "رفرش صفحه اصلی")]
    MaiPageResetForm
}