using System.Text.Json;
using Dapr.Client;
using Dapr.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDaprClient();

builder.Host.ConfigureAppConfiguration((configBuilder) =>
{
    var daprClient = new DaprClientBuilder().Build();
    configBuilder.AddDaprSecretStore("demosecrets", daprClient, TimeSpan.FromSeconds(10));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/secret", async (context) =>
{
    var secretValue = app.Configuration["super-secret"];

    context.Response.ContentType = "application/json";
    await JsonSerializer.SerializeAsync(context.Response.Body, secretValue);
});

app.Run();