using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.General;

public class EmailViewModel
{

    public int Id { get; set; }
    [Display(Name = "هاست")]
    [Required(ErrorMessage = "وارد کردن {0} الزامی است")]
    public string? Host { get; set; }
    [Display(Name = "پورت")]
    [Required(ErrorMessage = "وارد کردن {0} الزامی است")]
    public int Port { get; set; }
    public bool EnableSSL { get; set; }
    [Display(Name = "ایمیل ارسال کننده")]
    [Required(ErrorMessage = "وارد کردن {0} الزامی است")]
    public string? SenderEmail { get; set; }
    [Display(Name = "نام ارسال کننده")]
    [Required(ErrorMessage = "وارد کردن {0} الزامی است")]
    public string? SenderName { get; set; }
    public bool IsActive { get; set; }
    [Display(Name = "نام کاربری")]
    [Required(ErrorMessage = "وارد کردن {0} الزامی است")]
    public string? UserName { get; set; }
    [Display(Name = "کلمه عبور")]
    [Required(ErrorMessage = "وارد کردن {0} الزامی است")]
    public string? Password { get; set; }
}
