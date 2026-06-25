using ApiIveco;
using ApiIveco.Data;
using ApiIveco.Middlewares;       /// RequestResponseLoggingMiddleware
using ApiIveco.Service;
using ApiIveco.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "logs"));

var builder = WebApplication.CreateBuilder(args);


/// 1. CONFIGURAÇÃO DO SERILOG
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});


/// 2. DEPENDÊNCIAS
builder.Services.AddSingleton<FireBaseData>();
builder.Services.AddScoped<DadosService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IEmailValidationService, EmailValidationService>();


/// 3. SWAGGER
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Iveco Green Ledger",
        Version = "v1",
        Description = "API de Backend para gestão de Veículos, Fornecedores e Rastreabilidade.",
        Contact = new OpenApiContact
        {
            Name = "Equipe de Desenvolvimento",
            Email = "suporte@ivecogreenledger.com"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

/// 1. Ativa o uso do IMemoryCache (exigido pelo seu DadosService para gerenciar o "PegadaMediaCache")
builder.Services.AddMemoryCache();

///2. Registra a sua classe de conexão com o Firebase 
///(Recomendado como Singleton para não abrir múltiplas conexões desnecessárias com o Firestore)
builder.Services.AddSingleton<ApiIveco.Data.FireBaseData>();

// 3. Registra o serviço principal que faz a ponte entre os Controllers e o Banco de Dados
builder.Services.AddScoped<ApiIveco.Service.DadosService>();

var app = builder.Build();


/// 4. MIDDLEWARES (ORDEM CRÍTICA)
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

/// AQUI ESTÁ A PRIMEIRA CORREÇÃO: Removido o 'if (IsDevelopment)' 
/// para o Swagger aparecer na internet no runasp.net
app.UseSwagger();
app.UseSwaggerUI();

/// AQUI ESTÁ A SEGUNDA CORREÇÃO: Redireciona a rota raiz ("/") para o Swagger,
/// acabando com o Erro 404 ao acessar o link principal.
app.MapGet("/", () => Results.Redirect("/swagger"));


app.UseAuthorization();
app.MapControllers();


// ================================================================
// CARREGAMENTO DAS CREDENCIAIS DO FIREBASE (MÚLTIPLAS TENTATIVAS)
// ================================================================
string credPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
if (string.IsNullOrEmpty(credPath) || !File.Exists(credPath))
{
    string[] possiveisCaminhos = {
        Path.Combine(Directory.GetCurrentDirectory(), "chave_Api", "firebase-key.json"),
        Path.Combine(AppContext.BaseDirectory, "firebase-key.json"),
        Path.Combine(Directory.GetCurrentDirectory(), "firebase-key.json"),
        Path.Combine(Directory.GetCurrentDirectory(), "Credentials", "firebase-key.json")
    };

    foreach (var path in possiveisCaminhos)
    {
        if (File.Exists(path))
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            Console.WriteLine($"[INFO] Credenciais carregadas de: {path}");
            break;
        }
    }
}
else
{
    Console.WriteLine($"[INFO] Credenciais já definidas na variável: {credPath}");
}

/// 5. INÍCIO
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