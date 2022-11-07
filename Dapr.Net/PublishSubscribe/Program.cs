using Dapr.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PublishSubscribe;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<DaprClient>(sp =>
        {
            return new DaprClientBuilder()
                .UseHttpEndpoint($"http://127.0.0.1:{Environment.GetEnvironmentVariable("DAPR_HTTP_PORT")}")
                // .UseGrpcEndpoint($"http://127.0.0.1:{Environment.GetEnvironmentVariable("DAPR_GRPC_PORT")}")
                .Build();
        });

        services.AddTransient<PublishEventExample>();
    })
    .Build();

await host.Services.GetRequiredService<PublishEventExample>().RunAsync(default);

await host.RunAsync();