using ApiIveco.Data;
using ApiIveco.Service;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 1. FORÇAR A PORTA DO GOOGLE CLOUD RUN
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. CONFIGURAÇÃO SEGURA DO SWAGGER
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddScoped<DadosService>();
builder.Services.AddHttpClient<DadosService>();
builder.Services.AddScoped<FireBaseData>();

// 3. CONFIGURAÇÃO SEGURA DO FIREBASE (Caminho compatível com Linux e Windows)
var caminhoChave = Path.Combine(AppContext.BaseDirectory, "chave_API", "firebase-key.json");

if (File.Exists(caminhoChave))
{
    var credential = GoogleCredential.FromFile(caminhoChave);
    var firestoreData = new FirestoreDbBuilder
    {
        ProjectId = "green-leadger",
        Credential = credential,
    }.Build();
    builder.Services.AddSingleton(firestoreData);
}
else
{
    // A API não vai "quebrar", mas vai emitir este alerta no log do Google Cloud
    Console.WriteLine($"[AVISO CRÍTICO] O arquivo do Firebase não foi encontrado no servidor em: {caminhoChave}");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Iveco V1");
});

app.UseHttpsRedirection();
app.UseCors("PermitirTudo");
app.UseAuthorization();
app.MapControllers();

app.Run();