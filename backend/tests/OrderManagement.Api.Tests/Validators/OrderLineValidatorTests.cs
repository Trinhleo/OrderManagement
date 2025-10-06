using FluentAssertions;
using FluentValidation.TestHelper;
using OrderManagement.Application.Commands;
using OrderManagement.Application.Commands.Validators;
using Xunit;

namespace OrderManagement.Api.Tests.Validators;

public class PlaceOrderLineValidatorTests
{
    private readonly PlaceOrderLineValidator _validator;

    public PlaceOrderLineValidatorTests()
    {
        _validator = new PlaceOrderLineValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Product_Is_Empty()
    {
        // Arrange
        var orderLine = new PlaceOrderLine("", 1, 10.00m, "USD");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Product)
            .WithErrorMessage("Product name is required");
    }

    [Fact]
    public void Should_Have_Error_When_Product_Exceeds_MaxLength()
    {
        // Arrange
        var orderLine = new PlaceOrderLine(new string('A', 101), 1, 10.00m, "USD");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Product)
            .WithErrorMessage("Product name must not exceed 100 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Should_Have_Error_When_Quantity_Is_Not_Positive(int quantity)
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Product1", quantity, 10.00m, "USD");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_Quantity_Exceeds_Maximum()
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Product1", 10001, 10.00m, "USD");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Quantity)
            .WithErrorMessage("Quantity cannot exceed 10000");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.50)]
    public void Should_Have_Error_When_Price_Is_Not_Positive(decimal price)
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Product1", 1, price, "USD");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Unit price must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_Price_Exceeds_Maximum()
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Product1", 1, 1000001m, "USD");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Unit price cannot exceed 1,000,000");
    }

    [Fact]
    public void Should_Have_Error_When_Currency_Is_Empty()
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Product1", 1, 10.00m, "");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency is required");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("A")]
    public void Should_Have_Error_When_Currency_Length_Is_Invalid(string currency)
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Product1", 1, 10.00m, currency);

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency)
            .WithErrorMessage("Currency must be 3 characters (e.g., USD, EUR)");
    }

    [Fact]
    public void Should_Not_Have_Error_When_OrderLine_Is_Valid()
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Valid Product", 5, 25.99m, "USD");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Product")]
    [InlineData("A very long product name that is still valid")]
    public void Should_Not_Have_Error_When_Product_Is_Valid(string productName)
    {
        // Arrange
        var orderLine = new PlaceOrderLine(productName, 1, 10.00m, "EUR");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Product);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(10000)]
    public void Should_Not_Have_Error_When_Quantity_Is_Valid(int quantity)
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Product1", quantity, 10.00m, "GBP");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(99.99)]
    [InlineData(1000000)]
    public void Should_Not_Have_Error_When_Price_Is_Valid(decimal price)
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Product1", 1, price, "JPY");

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public void Should_Not_Have_Error_When_Currency_Is_Valid(string currency)
    {
        // Arrange
        var orderLine = new PlaceOrderLine("Product1", 1, 10.00m, currency);

        // Act
        var result = _validator.TestValidate(orderLine);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
    }
}