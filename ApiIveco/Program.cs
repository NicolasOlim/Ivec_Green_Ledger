using ApiIveco.Data; // Certifique-se de ter os usings corretos no topo
using ApiIveco.Service;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// ========================================================
// 1. CORREÇÃO DO ERRO DO CONSTRUTOR (INJEÇÃO DE DEPENDÊNCIA)
// ========================================================

// Remova qualquer linha que diga "AddHttpClient<DadosService>()" e use estas:
builder.Services.AddSingleton<FireBaseData>(); // Regista a ligação ao Firebase
builder.Services.AddScoped<DadosService>();    // Regista o seu serviço corretamente
builder.Services.AddHttpClient();              // Permite chamadas à internet genéricas

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ========================================================
// 2. ESCONDER ERROS DO SWAGGER E MOSTRAR SÓ NOS LOGS
// ========================================================
// Este bloco captura QUALQUER crash antes de chegar ao Swagger
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        // 1. Escreve o erro real APENAS na consola preta (Log)
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[ERRO CRÍTICO NO BACKEND]: {exception?.Message}\n");
        Console.ResetColor();

        // 2. Devolve um JSON limpo e amigável para o Swagger e para o WPF
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            Erro = "Falha Interna do Servidor",
            Mensagem = "Ocorreu um erro interno. Por favor, verifique a consola da API para ler os logs."
        });
    });
});

// Removemos a página de erro do desenvolvedor para garantir que o HTML nunca aparece
// if (app.Environment.IsDevelopment())
// {
//     app.UseDeveloperExceptionPage(); <- APAGUE OU COMENTE ISTO SE EXISTIR!
// }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();