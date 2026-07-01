using System.Net;
using System.Text.Json;
using API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock = new();

    private ExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new ExceptionHandlingMiddleware(next, _loggerMock.Object);
    }

    [Fact]
    public async Task Should_return_500_for_unhandled_exception()
    {
        var middleware = CreateMiddleware(_ => throw new Exception("Something went wrong"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task Should_return_401_for_UnauthorizedAccessException()
    {
        var middleware = CreateMiddleware(_ => throw new UnauthorizedAccessException());
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_return_404_for_KeyNotFoundException()
    {
        var middleware = CreateMiddleware(_ => throw new KeyNotFoundException());
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_return_400_for_ArgumentException()
    {
        var middleware = CreateMiddleware(_ => throw new ArgumentException("Invalid argument"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_return_400_for_InvalidOperationException()
    {
        var middleware = CreateMiddleware(_ => throw new InvalidOperationException("Invalid operation"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_return_json_with_success_false()
    {
        var middleware = CreateMiddleware(_ => throw new Exception("Error"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var json = JsonDocument.Parse(body);

        json.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        json.RootElement.GetProperty("message").GetString().Should().Be("An error occurred processing your request");
    }

    [Fact]
    public async Task Should_return_unauthorized_message_for_UnauthorizedAccessException()
    {
        var middleware = CreateMiddleware(_ => throw new UnauthorizedAccessException());
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var json = JsonDocument.Parse(body);

        json.RootElement.GetProperty("message").GetString().Should().Be("Unauthorized");
    }

    [Fact]
    public async Task Should_call_next_delegate_when_no_exception()
    {
        var called = false;
        var middleware = CreateMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}
