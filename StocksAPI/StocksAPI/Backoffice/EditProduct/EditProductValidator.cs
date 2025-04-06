using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;

namespace StocksAPI.Backoffice.EditProduct;

public class EditProductValidator : Validator<EditProductRequest>
{
    public EditProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(100)
            .WithMessage("Product name cannot exceed 100 characters")
            .MustAsync(async (request, name, ct) =>
            {
                var db = Resolve<StocksDbContext>();
                return !await db.Products
                    .AnyAsync(p => p.Name == name && p.Id != request.Id, ct);
            })
            .WithMessage("A product with this name already exists");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Invalid product category");
    }
}