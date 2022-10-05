using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Routing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDaprClient();

builder.Services.AddSingleton(new JsonSerializerOptions()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
});

const string StoreName = "statestore";
const string PubsubName = "pubsub";

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

var logger = app.Logger;
var serializerOptions = app.Services.GetRequiredService<JsonSerializerOptions>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapSubscribeHandler();

    var depositTopicOptions = new TopicOptions
    {
        PubsubName = PubsubName,
        Name = "deposit",
        DeadLetterTopic = "amountDeadLetterTopic"
    };

    var withdrawTopicOptions = new TopicOptions
    {
        PubsubName = PubsubName,
        Name = "withdraw",
        DeadLetterTopic = "amountDeadLetterTopic"
    };

    endpoints.MapGet("{id}", Balance);
    endpoints.MapPost("deposit", Deposit).WithTopic(PubsubName, "deposit");
    endpoints.MapPost("deadLetterTopicRoute", ViewErrorMessage).WithTopic(PubsubName, "amountDeadLetterTopic");
    endpoints.MapPost("withdraw", Withdraw).WithTopic(PubsubName, "withdraw");
});

app.Run();



async Task ViewErrorMessage(HttpContext context)
{
    var transaction = await JsonSerializer.DeserializeAsync<Transaction>(context.Request.Body, serializerOptions);

    logger.LogInformation("The amount cannot be negative: {0}", transaction.Amount);

    return;
}


async Task Balance(HttpContext context)
{
    logger.LogInformation("Enter balance");
    var client = context.RequestServices.GetRequiredService<DaprClient>();

    var id = (string?)context.Request.RouteValues["id"];
    if (id == null)
    {
        logger.LogWarning("Account id not provided");
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return;
    }

    var account = await client.GetStateAsync<Account>(StoreName, id);
    if (account == null)
    {
        logger.LogInformation("Account not found");
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        return;
    }

    logger.LogInformation("Account balance is {0}", account.Balance);

    context.Response.ContentType = "application/json";
    await JsonSerializer.SerializeAsync(context.Response.Body, account, serializerOptions);
}

async Task Deposit(HttpContext context)
{
    logger.LogInformation("Enter Deposit");

    var client = context.RequestServices.GetRequiredService<DaprClient>();
    var transaction = await JsonSerializer.DeserializeAsync<Transaction>(context.Request.Body, serializerOptions);

    logger.LogInformation("Id is {0}, Amount is {1}", transaction.Id, transaction.Amount);

    var account = await client.GetStateAsync<Account>(StoreName, transaction.Id);
    if (account == null)
    {
        logger.LogInformation("New Account!");
        account = new Account() { Id = transaction.Id, Balance = 0m };
    }

    if (transaction.Amount < 0m)
    {
        logger.LogInformation("Invalid amount");
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return;
    }

    account.Balance += transaction.Amount;
    await client.SaveStateAsync(StoreName, transaction.Id, account);
    logger.LogInformation("Balance is {0}", account.Balance);

    context.Response.ContentType = "application/json";
    await JsonSerializer.SerializeAsync(context.Response.Body, account, serializerOptions);
}

async Task Withdraw(HttpContext context) 
{
    logger.LogInformation("Enter withdraw");

    var client = context.RequestServices.GetRequiredService<DaprClient>();
    var transaction = await JsonSerializer.DeserializeAsync<Transaction>(context.Request.Body, serializerOptions);

    logger.LogInformation("Id is {id}, amount is {amount}", transaction.Id, transaction.Amount);

    var account = await client.GetStateAsync<Account>(StoreName, transaction.Id);
    if (account == null) 
    {
        logger.LogInformation("Account not found");
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        return;
    }

    if (transaction.Amount < 0m)
    {
        logger.LogInformation("Invalid amount");
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return;
    }

    account.Balance -= transaction.Amount;
    await client.SaveStateAsync(StoreName, transaction.Id, account);
    logger.LogInformation("Balance is {balance}", account.Balance);

    context.Response.ContentType = "application/json";
    await JsonSerializer.SerializeAsync(context.Response.Body, account, serializerOptions);
}

internal class Transaction
{
    public string? Id { get; set; }

    public decimal? Amount { get; set; }
}

internal class Account
{
    public string? Id { get; set; }

    public decimal? Balance { get; set; }
}