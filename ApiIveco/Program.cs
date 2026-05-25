using ApiIveco.Data;
using ApiIveco.Service;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Iveco V1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseCors("PermitirTudo");

app.UseAuthorization();

app.MapControllers();

app.Run();