using FastEndpoints;
using FluentValidation;

namespace StocksAPI.Backoffice.DeleteColor;

public class DeleteColorValidator : Validator<DeleteColorRequest>
{
    public DeleteColorValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ColorId)
            .NotEmpty()
            .WithMessage("Color ID is required");
    }
}