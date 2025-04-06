using FastEndpoints;
using FluentValidation;

namespace StocksAPI.Backoffice.CreateProduct;

public class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MinimumLength(2).WithMessage("Product name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid product category");
    }
}