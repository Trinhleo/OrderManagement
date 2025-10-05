using FluentAssertions;
using FluentValidation.TestHelper;
using OrderManagement.Application.Commands;
using OrderManagement.Application.Commands.Validators;
using System.Collections.Generic;
using Xunit;

namespace OrderManagement.Api.Tests.Validators;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator;

    public PlaceOrderCommandValidatorTests()
    {
        _validator = new PlaceOrderCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_CustomerName_Is_Empty()
    {
        // Arrange
        var command = new PlaceOrderCommand("", new List<PlaceOrderLine>
        {
            new("Product1", 1, 10.00m, "USD")
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("Customer name is required");
    }

    [Fact]
    public void Should_Have_Error_When_CustomerName_Exceeds_MaxLength()
    {
        // Arrange
        var command = new PlaceOrderCommand(new string('A', 201), new List<PlaceOrderLine>
        {
            new("Product1", 1, 10.00m, "USD")
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerName)
            .WithErrorMessage("Customer name must not exceed 200 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Lines_Is_Empty()
    {
        // Arrange
        var command = new PlaceOrderCommand("John Doe", new List<PlaceOrderLine>());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Lines)
            .WithErrorMessage("At least one order line is required");
    }

    [Fact]
    public void Should_Have_Error_When_Lines_Exceeds_MaxCount()
    {
        // Arrange
        var orderLines = new List<PlaceOrderLine>();
        for (int i = 0; i < 101; i++) // 101 order lines
        {
            orderLines.Add(new PlaceOrderLine($"Product{i}", 1, 10.00m, "USD"));
        }

        var command = new PlaceOrderCommand("John Doe", orderLines);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Lines)
            .WithErrorMessage("Maximum 100 order lines allowed");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new PlaceOrderCommand("John Doe", new List<PlaceOrderLine>
        {
            new("Product1", 2, 15.50m, "USD"),
            new("Product2", 1, 25.00m, "EUR")
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("John")]
    [InlineData("John Doe")]
    [InlineData("A very long customer name that is still within the limit")]
    public void Should_Not_Have_Error_When_CustomerName_Is_Valid(string customerName)
    {
        // Arrange
        var command = new PlaceOrderCommand(customerName, new List<PlaceOrderLine>
        {
            new("Product1", 1, 10.00m, "USD")
        });

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerName);
    }
}