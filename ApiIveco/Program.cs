using ApiIveco.Data;
using ApiIveco.Service;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Localiza o ficheiro XML gerado pelo .csproj
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    // Diz ao Swagger para usar este ficheiro XML
    options.IncludeXmlComments(xmlPath);
});
// Injeção de Dependências
builder.Services.AddScoped<DadosService>();
builder.Services.AddHttpClient<DadosService>();
builder.Services.AddScoped<FireBaseData>();

// Configuração do Firebase
var caminhoChave = Path.Combine(Directory.GetCurrentDirectory(), "chave_API/firebase-key.json");
var credential = GoogleCredential.FromFile(caminhoChave);

var firestoreData = new FirestoreDbBuilder
{
    ProjectId = "green-leadger",
    Credential = credential,
}.Build();

builder.Services.AddSingleton(firestoreData);

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build(); // Chamado apenas UMA vez aqui


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