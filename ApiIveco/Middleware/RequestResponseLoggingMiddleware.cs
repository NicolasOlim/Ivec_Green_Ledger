using System.Diagnostics;
using System.Text;
using Microsoft.IO;

namespace ApiIveco.Middlewares;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly IWebHostEnvironment _env;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        /// LOG DE ENTRADA (APENAS MÉTODO E PATH)
        _logger.LogInformation("➡️ {Method} {Path}", context.Request.Method, context.Request.Path);

        /// EM DESENVOLVIMENTO, MOSTRA O CORPO DA REQUISIÇÃO (OPCIONAL)
        if (_env.IsDevelopment())
        {
            var requestBody = await FormatRequest(context.Request);
            if (!string.IsNullOrEmpty(requestBody))
                _logger.LogDebug("📦 Request Body: {Body}", requestBody);
        }

        var originalBodyStream = context.Response.Body;
        await using var responseBody = _recyclableMemoryStreamManager.GetStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var status = context.Response.StatusCode;

            string emoji = status >= 500 ? "💥" : status >= 400 ? "⚠️" : "✅";
            LogLevel logLevel = status >= 400 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(logLevel, $"{emoji} {status} ({stopwatch.ElapsedMilliseconds}ms)");

            /// EM DESENVOLVIMENTO, MOSTRA O CORPO DA RESPOSTA
            if (_env.IsDevelopment())
            {
                var responseBodyText = await FormatResponse(context.Response);
                if (!string.IsNullOrEmpty(responseBodyText))
                    _logger.LogDebug("📦 Response Body: {Body}", responseBodyText);
            }

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(request.ContentLength ?? 0)];
        await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        request.Body.Position = 0;
        return bodyAsText;
    }

    private async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        return text;
    }
}