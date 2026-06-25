using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ApiIveco
{
    /// <summary>
    /// Middleware global para capturar exceções não tratadas e 
    /// retornar respostas padronizadas ao cliente, enquanto registra 
    /// o erro completo nos logs para diagnóstico.
    /// </summary>
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
                // 1. Log COMPLETO da exceção (com stack trace) para diagnóstico
                _logger.LogError(ex,
                    "[ERRO] {Method} {Path} -> {Tipo}: {Mensagem}\nStackTrace: {StackTrace}",
                    context.Request.Method,
                    context.Request.Path,
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace);

                // 2. Se houver inner exception, logue também
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Inner} - {InnerMsg}\n{InnerStack}",
                        ex.InnerException.GetType().Name,
                        ex.InnerException.Message,
                        ex.InnerException.StackTrace);
                }

                // 3. Escreve resposta genérica para o cliente
                await EscreverRespostaGenerica(context, ex);
            }
        }

        private static async Task EscreverRespostaGenerica(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            // Define o status HTTP baseado no tipo de exceção
            context.Response.StatusCode = ex switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                InvalidOperationException => (int)HttpStatusCode.UnprocessableEntity,
                HttpRequestException => (int)HttpStatusCode.BadGateway,
                _ => (int)HttpStatusCode.InternalServerError
            };

            // Para erros 400 ou 422, enviamos a mensagem real; 
            // para outros, mensagem genérica.
            bool enviarMensagemReal = context.Response.StatusCode == 400 || context.Response.StatusCode == 422;

            var resposta = new
            {
                Status = context.Response.StatusCode,
                Mensagem = enviarMensagemReal ? ex.Message : ObterMensagemGenerica(context.Response.StatusCode),
                // Em desenvolvimento, pode incluir mais detalhes
                // Detalhe = enviarMensagemReal ? ex.StackTrace : null
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