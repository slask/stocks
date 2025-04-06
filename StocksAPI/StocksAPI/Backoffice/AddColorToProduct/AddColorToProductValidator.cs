using FastEndpoints;
using FluentValidation;

namespace StocksAPI.Backoffice.AddColorToProduct;

public class AddColorToProductValidator : Validator<AddColorToProductRequest>
{
    public AddColorToProductValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Color code is required")
            .MaximumLength(20).WithMessage("Color code cannot exceed 20 characters")
            .Matches("^[A-Za-z0-9]+$").WithMessage("Color code can only contain letters and numbers");

        RuleFor(x => x.ExistingQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative");
    }
}