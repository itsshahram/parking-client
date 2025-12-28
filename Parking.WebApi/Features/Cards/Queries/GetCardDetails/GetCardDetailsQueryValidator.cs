using FluentValidation;

namespace Parking.WebApi.Features.Cards.Queries.GetCardDetails;

public class GetCardDetailsQueryValidator : AbstractValidator<GetCardDetailsQuery>
{
    public GetCardDetailsQueryValidator()
    {
        RuleFor(x => x.CardUid)
            .GreaterThan(0).WithMessage("شناسه کارت باید بزرگتر از صفر باشد");
    }
}