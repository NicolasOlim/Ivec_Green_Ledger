using ApiIveco.Data;
using ApiIveco.Service;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models; // <-- NECESSÁRIO PARA O OPENAPI/SWAGGER
using System.Reflection;        // <-- NECESSÁRIO PARA LER O FICHEIRO XML

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders(); // Limpa a formatação feia padrão
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true; // Força o log a ficar numa única linha (muito mais fácil de ler)
    options.TimestampFormat = "[HH:mm:ss] "; // Adiciona a hora exata na frente
    options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled; // Mantém as cores (Verde, Amarelo, Vermelho)
});

// ========================================================
// 1. CORREÇÃO DO ERRO DO CONSTRUTOR (INJEÇÃO DE DEPENDÊNCIA)
// ========================================================
builder.Services.AddSingleton<FireBaseData>();
builder.Services.AddScoped<DadosService>();
builder.Services.AddHttpClient();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ========================================================
// 3. CONFIGURAÇÃO AVANÇADA DO SWAGGER (DOCUMENTAÇÃO)
// ========================================================
builder.Services.AddSwaggerGen(options =>
{
    // Informações da página inicial do Swagger
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Iveco Green Ledger",
        Version = "v1",
        Description = "API de Backend para a gestão de Veículos, Fornecedores e Rastreabilidade do projeto Iveco Green Ledger.",
        Contact = new OpenApiContact
        {
            Name = "Equipa de Desenvolvimento",
            Email = "suporte@ivecogreenledger.com"
        }
    });

    // Configuração para ler os comentários /// (Summary) do código
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    // Verifica se o ficheiro existe para não crashar caso te esqueças do Passo 2
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ========================================================
// 2. ESCONDER ERROS DO SWAGGER E MOSTRAR SÓ NOS LOGS
// ========================================================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[ERRO CRÍTICO NO BACKEND]: {exception?.Message}\n");
        Console.ResetColor();

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            Erro = "Falha Interna do Servidor",
            Mensagem = "Ocorreu um erro interno. Por favor, verifique a consola da API para ler os logs."
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Personaliza a interface do Swagger UI
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Iveco v1");
        // options.RoutePrefix = string.Empty; // <- Descomenta isto se quiseres que o Swagger abra logo no http://localhost:44353/ (sem ter que escrever /swagger)
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();