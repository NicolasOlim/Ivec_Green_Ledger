using ApiIveco;
using ApiIveco.Data;
using ApiIveco.Middlewares;       // RequestResponseLoggingMiddleware
using ApiIveco.Service;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ========================================================
// 1. CONFIGURAÇÃO DO SERILOG
// ========================================================
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// ========================================================
// 2. DEPENDÊNCIAS
// ========================================================
builder.Services.AddSingleton<FireBaseData>();
builder.Services.AddScoped<DadosService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ========================================================
// 3. SWAGGER
// ========================================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Iveco Green Ledger",
        Version = "v1",
        Description = "API de Backend para gestão de Veículos, Fornecedores e Rastreabilidade.",
        Contact = new OpenApiContact
        {
            Name = "Equipa de Desenvolvimento",
            Email = "suporte@ivecogreenledger.com"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ========================================================
// 4. MIDDLEWARES (ORDEM CRÍTICA)
// ========================================================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
        Log.Error(exception, "💥 ERRO CRÍTICO NÃO TRATADO");
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            Erro = "Falha Interna do Servidor",
            Mensagem = "Ocorreu um erro interno. Verifique os logs da API."
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestResponseLoggingMiddleware>(); // Loga requisições/respostas
app.UseMiddleware<ExceptionMiddleware>();              // Seu middleware personalizado

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ========================================================
// 5. INÍCIO
// ========================================================
try
{
    Log.Information("🚀 API Iveco Green Ledger iniciada com sucesso!");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Falha fatal durante a execução da API");
    throw;
}
finally
{
    Log.CloseAndFlush();
}