using Dapr.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class InvokeServiceHttpClientExample : Example
    {
        private readonly ILogger<InvokeServiceHttpClientExample> _logger;

        public InvokeServiceHttpClientExample(ILogger<InvokeServiceHttpClientExample> logger)
        {
            _logger = logger;
        }

        public override string DisplayName => "Invoking an HTTP Service with DaprClient";

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            var httpClient = DaprClient.CreateInvokeHttpClient(appId: "routing");
            _logger.LogInformation("Running {example}", nameof(InvokeServiceHttpClientExample));

            _logger.LogInformation($"Invoking deposit");
            var data = new Transaction { Id = "17", Amount = 100m };
            var response = await httpClient.PostAsJsonAsync("/deposit", data, cancellationToken);
            var account = await response.Content.ReadFromJsonAsync<Account>(cancellationToken: cancellationToken);
            _logger.LogInformation("Returned: Id:{id} | Balance:{balance}", account?.Id, account?.Balance);

            _logger.LogInformation($"Invoking withdraw");
            var withdraw = new Transaction { Id = "17", Amount = 10m };
            response = await httpClient.PostAsJsonAsync("/withdraw", withdraw, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Completed");

            _logger.LogInformation($"Invoking balance");
            response = await httpClient.GetAsync("/17");
            account = await response.Content.ReadFromJsonAsync<Account>(cancellationToken: cancellationToken);
            _logger.LogInformation("Received balance {balance}", account?.Balance);
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
    }
}
