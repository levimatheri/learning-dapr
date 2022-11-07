using Dapr.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublishSubscribe
{
    public class PublishEventExample
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<PublishEventExample> _logger;

        public PublishEventExample(DaprClient daprClient, ILogger<PublishEventExample> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var eventData = new { Id = "17", Amount = 10m };
            await _daprClient.PublishEventAsync("pubsub", "deposit", eventData, cancellationToken);
            _logger.LogInformation("Published deposit event");
        }
    }
}
