using Dapr.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class InvokeServiceHttpExample : Example
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<InvokeServiceHttpExample> _logger;

        public InvokeServiceHttpExample(DaprClient daprClient, ILogger<InvokeServiceHttpExample> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }

        public override string DisplayName => "Invoking an HTTP Service with DaprClient";

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running {example}", nameof(InvokeServiceHttpExample));

            // _logger.LogInformation($"Invoking deposit");
            // var data = new Transaction { Id = "17", Amount = 99m };
            // var account = await _daprClient.InvokeMethodAsync<Transaction, Account>("routing", "deposit", data, cancellationToken);
            // _logger.LogInformation("Returned: Id:{id} | Balance:{balance}", account.Id, account.Balance);

            _logger.LogInformation($"Invoking withdraw");
            var data = new Transaction { Id = "17", Amount = 311m };
            await _daprClient.InvokeMethodAsync<Transaction, Account>("routing", "withdraw", data, cancellationToken);
            _logger.LogInformation("Completed");

            _logger.LogInformation($"Invoking balance");
            var account = await _daprClient.InvokeMethodAsync<Account>(HttpMethod.Get, "routing", "17", cancellationToken);
            _logger.LogInformation("Received balance {balance}", account.Balance);
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
