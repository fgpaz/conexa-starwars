using ConexaStarWars.API.Middleware;
using ConexaStarWars.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;
using CoreNotFoundException = ConexaStarWars.Core.Exceptions.NotFoundException;

namespace ConexaStarWars.Tests.Middleware;

public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _mockLogger;
    private readonly DefaultHttpContext _context;

    public ErrorHandlingMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<ErrorHandlingMiddleware>>();
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_WithNoException_ShouldCallNext()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx) 
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var middleware = new ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithValidationException_ShouldReturn400()
    {
        // Arrange
        var errors = new { Field = "Error message" };
        
        Task next(HttpContext ctx) => throw new ValidationException(errors);
        
        var middleware = new ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(400, _context.Response.StatusCode);
        Assert.Equal("application/json", _context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WithNotFoundException_ShouldReturn404()
    {
        // Arrange
        Task next(HttpContext ctx) => throw new CoreNotFoundException("Resource not found");
        
        var middleware = new ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(404, _context.Response.StatusCode);
        Assert.Equal("application/json", _context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WithForbiddenException_ShouldReturn403()
    {
        // Arrange
        Task next(HttpContext ctx) => throw new ForbiddenException("Access denied");
        
        var middleware = new ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(403, _context.Response.StatusCode);
        Assert.Equal("application/json", _context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WithArgumentException_ShouldReturn400()
    {
        // Arrange
        Task next(HttpContext ctx) => throw new ArgumentException("Invalid argument");
        
        var middleware = new ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(400, _context.Response.StatusCode);
        Assert.Equal("application/json", _context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthorizedAccessException_ShouldReturn401()
    {
        // Arrange
        Task next(HttpContext ctx) => throw new UnauthorizedAccessException("Unauthorized");
        
        var middleware = new ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(401, _context.Response.StatusCode);
        Assert.Equal("application/json", _context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WithGenericException_ShouldReturn500()
    {
        // Arrange
        Task next(HttpContext ctx) => throw new Exception("Generic error");
        
        var middleware = new ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(500, _context.Response.StatusCode);
        Assert.Equal("application/json", _context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidOperationException_ShouldReturn500()
    {
        // Arrange
        Task next(HttpContext ctx) => throw new InvalidOperationException("Invalid operation");
        
        var middleware = new ErrorHandlingMiddleware(next, _mockLogger.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.Equal(500, _context.Response.StatusCode);
        Assert.Equal("application/json", _context.Response.ContentType);
    }
} 