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
                ArgumentException => (int)HttpStatusCode.BadRequest, /// 400
                KeyNotFoundException => (int)HttpStatusCode.NotFound, /// 404
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, /// 401
                InvalidOperationException => (int)HttpStatusCode.UnprocessableEntity, /// 422
                HttpRequestException => (int)HttpStatusCode.BadGateway, /// 502
                _ => (int)HttpStatusCode.InternalServerError /// 500
            };

            //Se for erro 400 ou 422, enviamos a mensagem real da exceção.
            bool enviarMensagemReal = context.Response.StatusCode == 400 || context.Response.StatusCode == 422;

            var resposta = new
            {
                Status = context.Response.StatusCode,
                Mensagem = enviarMensagemReal ? ex.Message : ObterMensagemGenerica(context.Response.StatusCode)
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