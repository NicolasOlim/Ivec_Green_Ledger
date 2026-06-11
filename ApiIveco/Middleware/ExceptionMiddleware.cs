using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ApiIveco
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                /// Erro REAL aparece só no Output/terminal da API
                _logger.LogError(ex,
                    "[ERRO] {Method} {Path} -> {Tipo}: {Mensagem}",
                    context.Request.Method,
                    context.Request.Path,
                    ex.GetType().Name,
                    ex.Message);

                /// Mensagem GENÉRICA vai para o cliente (WPF)
                await EscreverRespostaGenerica(context, ex);
            }
        }

        private static async Task EscreverRespostaGenerica(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = ex switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                InvalidOperationException => (int)HttpStatusCode.UnprocessableEntity,
                HttpRequestException => (int)HttpStatusCode.BadGateway,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var resposta = new
            {
                Status = context.Response.StatusCode,
                Mensagem = ObterMensagemGenerica(context.Response.StatusCode)
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(resposta));
        }

        private static string ObterMensagemGenerica(int statusCode) => statusCode switch
        {
            400 => "Os dados enviados são inválidos. Verifique e tente novamente.",
            401 => "Credenciais incorretas. Verifique o e-mail e a senha.",
            404 => "O recurso solicitado não foi encontrado.",
            422 => "A operação não pôde ser concluída. Verifique as informações.",
            502 => "Serviço externo indisponível. Tente novamente mais tarde.",
            _ => "Ocorreu um erro inesperado. Tente novamente ou contacte o suporte."
        };
    }
}