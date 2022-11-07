using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddDapr();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DaprClient>(sp =>
{
    return new DaprClientBuilder()
        .UseHttpEndpoint($"http://127.0.0.1:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}")
        .UseGrpcEndpoint($"http://127.0.0.1:{Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")}")
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCloudEvents();

app.UseAuthorization();

app.MapSubscribeHandler();
app.MapControllers();

app.Run();
