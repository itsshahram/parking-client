using FluentValidation;

namespace Parking.WebApi.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("نام کاربری الزامی است")
            .MaximumLength(100).WithMessage("نام کاربری نمی‌تواند بیشتر از 100 کاراکتر باشد");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است")
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل 6 کاراکتر باشد");
    }
}
