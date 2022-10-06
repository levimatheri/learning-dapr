using ConsoleApp;
using Dapr.Client;
using Google.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<DaprClient>(sp =>
        {
            var httpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT");
            return new DaprClientBuilder()
                .UseHttpEndpoint($"http://127.0.0.1:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}")
                .Build();
        });

        // services.AddHttpClient("Routing", httpClient => {
        //     httpClient = DaprClient.CreateInvokeHttpClient("routing");
        // });

        //services.AddTransient<InvokeServiceHttpExample>();
        services.AddTransient<InvokeServiceHttpClientExample>();
    })
    .Build();

//await host.Services.GetRequiredService<InvokeServiceHttpExample>().RunAsync(default);
await host.Services.GetRequiredService<InvokeServiceHttpClientExample>().RunAsync(default);

await host.RunAsync();