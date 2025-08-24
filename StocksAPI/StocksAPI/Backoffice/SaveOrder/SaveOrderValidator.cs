using FastEndpoints;
using FluentValidation;

namespace StocksAPI.Backoffice.SaveOrder;

public class SaveOrderValidator : Validator<SaveOrderRequest>
{
    public SaveOrderValidator()
    {
        RuleFor(x => x.ClientName)
            .NotEmpty()
            .WithMessage("Client name is required")
            .MaximumLength(100)
            .WithMessage("Client name cannot exceed 100 characters");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one order item is required");

        RuleForEach(x => x.Items).SetValidator(new OrderItemValidator());
    }
}

public class OrderItemValidator : Validator<OrderItemDto>
{
    public OrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.ColorId)
            .NotEmpty()
            .WithMessage("Color ID is required");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required");

        RuleFor(x => x.ColorCode)
            .NotEmpty()
            .WithMessage("Color code is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");
    }
}
