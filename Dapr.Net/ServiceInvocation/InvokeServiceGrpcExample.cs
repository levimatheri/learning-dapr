using Dapr.Client;
using Microsoft.Extensions.Logging;
using GrpcService.Generated;

namespace ConsoleApp
{
    public class InvokeServiceGrpcExample : Example
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<InvokeServiceHttpExample> _logger;

        public InvokeServiceGrpcExample(DaprClient dapr_daprClient, ILogger<InvokeServiceHttpExample> logger)
        {
            _daprClient = dapr_daprClient;
            _logger = logger;
        }

        public override string DisplayName => "Invoking a gRPC service with gRPC semantics and Protobuf with Dapr_daprClient";

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running {example}", nameof(InvokeServiceHttpExample));

            _logger.LogInformation($"Invoking grpc deposit");
            var deposit = new GrpcService.Generated.Transaction { Id = "17", Amount = 99 };
            var account = await _daprClient.InvokeMethodGrpcAsync<GrpcService.Generated.Transaction, GrpcService.Generated.Account>("grpcsample", "deposit", deposit, cancellationToken);
            _logger.LogInformation("Returned: Id:{id} | Balance:{balance}", account.Id, account.Balance);
            _logger.LogInformation("Completed grpc deposit");

            _logger.LogInformation($"Invoking grpc withdraw");
            var withdraw = new GrpcService.Generated.Transaction { Id = "17", Amount = 10 };
            await _daprClient.InvokeMethodGrpcAsync("grpcsample", "withdraw", withdraw, cancellationToken);
            _logger.LogInformation("Completed grpc withdraw");

            _logger.LogInformation($"Invoking grpc balance");
            var request = new GrpcService.Generated.GetAccountRequest { Id = "17" };
            account = await _daprClient.InvokeMethodGrpcAsync<GrpcService.Generated.GetAccountRequest, GrpcService.Generated.Account>("grpcsample", "getaccount", request, cancellationToken);
            _logger.LogInformation("Received grpc balance {balance}", account.Balance);
        }
    }
}
