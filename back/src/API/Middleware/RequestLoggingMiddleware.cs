using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private static readonly string[] SensitiveFields = ["password", "newPassword", "currentPassword", "accessToken", "refreshToken", "token"];

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    private static string RedactSensitiveData(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return body;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var redacted = RedactNode(doc.RootElement);
            return JsonSerializer.Serialize(redacted, new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            return body;
        }
    }

    private static JsonElement? RedactNode(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var dict = new Dictionary<string, JsonElement?>();
            foreach (var prop in element.EnumerateObject())
            {
                if (SensitiveFields.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                {
                    dict[prop.Name] = JsonDocument.Parse("\"***REDACTED***\"").RootElement.Clone();
                }
                else
                {
                    dict[prop.Name] = RedactNode(prop.Value);
                }
            }
            var json = JsonSerializer.Serialize(dict.ToDictionary(kv => kv.Key, kv => kv.Value));
            return JsonDocument.Parse(json).RootElement.Clone();
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            var arr = element.EnumerateArray().Select(RedactNode).ToList();
            var json = JsonSerializer.Serialize(arr);
            return JsonDocument.Parse(json).RootElement.Clone();
        }

        return element.Clone();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        context.Request.EnableBuffering();

        var method = context.Request.Method;
        var path = context.Request.Path;
        var query = context.Request.QueryString.ToString();
        var correlationId = Activity.Current?.Id ?? context.TraceIdentifier;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        string? requestBody = null;
        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            context.Response.Body = originalBodyStream;

            responseBodyStream.Position = 0;
            string? responseBody = null;
            if (responseBodyStream.Length > 0)
            {
                using var reader = new StreamReader(responseBodyStream, Encoding.UTF8);
                responseBody = await reader.ReadToEndAsync();
                responseBodyStream.Position = 0;
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }

            var redactedRequest = requestBody != null ? RedactSensitiveData(requestBody) : null;
            var redactedResponse = responseBody != null ? RedactSensitiveData(responseBody) : null;

            _logger.LogInformation(
                "[{CorrelationId}] {Method} {Path}{Query} responded {StatusCode} in {ElapsedMs}ms\n  Request: {RequestBody}\n  Response: {ResponseBody}",
                correlationId, method, path, query,
                context.Response.StatusCode, stopwatch.ElapsedMilliseconds,
                redactedRequest ?? "(empty)", redactedResponse ?? "(empty)");
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        => builder.UseMiddleware<RequestLoggingMiddleware>();
}
