using FluentValidation;

namespace OrderManagement.Application.Commands.Validators;

public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("At least one order line is required")
            .Must(lines => lines != null && lines.Count() <= 100)
            .WithMessage("Maximum 100 order lines allowed");

        RuleForEach(x => x.Lines).SetValidator(new PlaceOrderLineValidator());
    }
}

public class PlaceOrderLineValidator : AbstractValidator<PlaceOrderLine>
{
    public PlaceOrderLineValidator()
    {
        RuleFor(x => x.Product)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Quantity cannot exceed 10000");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0")
            .LessThanOrEqualTo(1000000).WithMessage("Unit price cannot exceed 1,000,000");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters (e.g., USD, EUR)");
    }
}