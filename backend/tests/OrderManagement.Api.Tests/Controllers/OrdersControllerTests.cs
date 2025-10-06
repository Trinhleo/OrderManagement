using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderManagement.Api.Controllers;
using OrderManagement.Application.Commands;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrderManagement.Api.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new OrdersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task PlaceOrder_WithValidCommand_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var command = new PlaceOrderCommand("John Doe", new List<PlaceOrderLine>
        {
            new("Product1", 2, 15.50m, "USD")
        });

        var expectedOrderId = Guid.NewGuid();
        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrderId);

        // Act
        var result = await _controller.PlaceOrder(command);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(OrdersController.GetOrder));
        createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(expectedOrderId);
        createdResult.Value.Should().BeEquivalentTo(new { OrderId = expectedOrderId });
    }

    [Fact]
    public async Task PlaceOrder_WhenMediatorThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new PlaceOrderCommand("John Doe", new List<PlaceOrderLine>
        {
            new("Product1", 2, 15.50m, "USD")
        });

        _mediatorMock
            .Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var action = async () => await _controller.PlaceOrder(command);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task GetOrder_WithValidId_ShouldReturnOkWithOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new Order
        {
            Id = orderId,
            CustomerName = "John Doe",
            OrderDate = DateTime.UtcNow,
            TotalAmount = 31.00m
        };

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetOrderQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedOrder);
    }

    [Fact]
    public async Task GetOrder_WhenOrderNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetOrderQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetOrder_WhenMediatorThrows_ShouldPropagateException()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(x => x.Send(It.Is<GetOrderQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var action = async () => await _controller.GetOrder(orderId);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task PlaceOrder_ShouldCallMediatorWithCorrectCommand()
    {
        // Arrange
        var command = new PlaceOrderCommand("Jane Smith", new List<PlaceOrderLine>
        {
            new("Product A", 1, 25.00m, "USD"),
            new("Product B", 3, 10.00m, "EUR")
        });

        var expectedOrderId = Guid.NewGuid();
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<PlaceOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrderId);

        // Act
        await _controller.PlaceOrder(command);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<PlaceOrderCommand>(c =>
                    c.CustomerName == command.CustomerName &&
                    c.Lines.Count() == command.Lines.Count() &&
                    c.Lines.First().Product == "Product A"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrder_ShouldCallMediatorWithCorrectQuery()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new Order
        {
            Id = orderId,
            CustomerName = "Test Customer",
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100.00m
        };

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetOrderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        await _controller.GetOrder(orderId);

        // Assert
        _mediatorMock.Verify(
            x => x.Send(
                It.Is<GetOrderQuery>(q => q.OrderId == orderId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}