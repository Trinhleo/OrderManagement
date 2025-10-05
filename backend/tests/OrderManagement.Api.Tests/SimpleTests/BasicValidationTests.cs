using Xunit;
using FluentAssertions;
using OrderManagement.Application.Commands.Validators;
using OrderManagement.Application.Commands;
using System.Collections.Generic;

namespace OrderManagement.Api.Tests.SimpleTests;

public class BasicValidationTests
{
    [Fact]
    public void PlaceOrderCommandValidator_ValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new PlaceOrderCommandValidator();
        var command = new PlaceOrderCommand("John Doe", new List<PlaceOrderLine>
        {
            new("Valid Product", 1, 10.00m, "USD")
        });

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PlaceOrderCommandValidator_EmptyCustomerName_ShouldHaveError()
    {
        // Arrange
        var validator = new PlaceOrderCommandValidator();
        var command = new PlaceOrderCommand("", new List<PlaceOrderLine>
        {
            new("Valid Product", 1, 10.00m, "USD")
        });

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Customer name is required");
    }

    [Fact]
    public void PlaceOrderLineValidator_ValidLine_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new PlaceOrderLineValidator();
        var orderLine = new PlaceOrderLine("Valid Product", 1, 10.00m, "USD");

        // Act
        var result = validator.Validate(orderLine);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void PlaceOrderLineValidator_InvalidQuantity_ShouldHaveError()
    {
        // Arrange
        var validator = new PlaceOrderLineValidator();
        var orderLine = new PlaceOrderLine("Valid Product", 0, 10.00m, "USD");

        // Act
        var result = validator.Validate(orderLine);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Quantity must be greater than 0");
    }

    [Fact]
    public void PlaceOrderLineValidator_InvalidPrice_ShouldHaveError()
    {
        // Arrange
        var validator = new PlaceOrderLineValidator();
        var orderLine = new PlaceOrderLine("Valid Product", 1, -5.00m, "USD");

        // Act
        var result = validator.Validate(orderLine);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Unit price must be greater than 0");
    }
}