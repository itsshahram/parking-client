using System.ComponentModel.DataAnnotations;


namespace Parking.App.Models.Login;

public class LoginRequest
{
    [Required(ErrorMessage = "نام کاربری الزامی است")]
    public string? Username { get; set; }
    [Required(ErrorMessage = "وارد کردن رمز عبور الزامی است")]
    public string? Password { get; set; }
}